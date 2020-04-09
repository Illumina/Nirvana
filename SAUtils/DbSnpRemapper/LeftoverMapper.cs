using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compression.Utilities;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using Variants;

namespace SAUtils.DbSnpRemapper
{
    public sealed class LeftoverMapper
    {
        private readonly StreamReader _leftoverReader;
        private readonly StreamReader _destReader;
        private readonly Dictionary<string, StreamWriter> _writers;
        private readonly ISequenceProvider _desSequenceProvider;

        public LeftoverMapper(StreamReader leftoverReader, StreamReader destReader, Dictionary<string, StreamWriter> writers,
            ISequenceProvider desSequenceProvider)
        {
            _leftoverReader = leftoverReader;
            _destReader = destReader;
            _writers = writers;
            _desSequenceProvider = desSequenceProvider;
        }

        public int Map()
        {
            // write out the relocated locations of the leftover rsIds whenever possible
            //reading in the leftover ids
            var leftoverIds = new HashSet<(long,string)>();
            Console.Write("Loading leftover ids...");
            string line;
            while ((line = _leftoverReader.ReadLine()) != null)
            {
                var splits = line.Split('#', 3);
                var id = long.Parse(splits[0]);
                var alt = splits[1];
                leftoverIds.Add((id, alt));
            }
            Console.WriteLine($"{leftoverIds.Count} found.");

            // stream through the dest file to find locations
            var leftoversWithDest = new Dictionary<(long, string), List<GenomicLocation>>();
            var currentChromName = "";
            while ((line = _destReader.ReadLine()) != null)
            {
                if (line.OptimizedStartsWith('#')) continue;
                
                var splits = line.Split('\t', VcfCommon.InfoIndex);
                var chromName = splits[VcfCommon.ChromIndex];
                if (chromName != currentChromName)
                {
                    currentChromName = chromName;
                    Console.WriteLine($"Getting destinations for chromosome:{currentChromName}...");
                    var currentChrom = ReferenceNameUtilities.GetChromosome(_desSequenceProvider.RefNameToChromosome,
                        currentChromName);
                    _desSequenceProvider.LoadChromosome(currentChrom);
                }

                var refAllele  = splits[VcfCommon.RefIndex];
                var altAlleles = splits[VcfCommon.AltIndex].Split(',');
                var position   = int.Parse(splits[VcfCommon.PosIndex]);
                var rsIds      = Utilities.GetRsids(splits[VcfCommon.IdIndex]);
                if (rsIds == null) continue;
                
                var processedVariants = altAlleles.Select(x => VariantUtils.TrimAndLeftAlign(position, refAllele, x, _desSequenceProvider.Sequence)).ToArray();

                foreach (var (_, _, variantAlt) in processedVariants)
                foreach (var rsId in rsIds)
                {
                    if (!leftoverIds.Contains((rsId, variantAlt))) continue;
                    var pos = int.Parse(splits[VcfCommon.PosIndex]);
                    if (!leftoversWithDest.TryGetValue((rsId, variantAlt), out var locations))
                    {
                        locations = new List<GenomicLocation>();
                        leftoversWithDest[(rsId, variantAlt)] = locations;
                    }
                    locations.Add(new GenomicLocation(chromName, pos));
                }

            }

            WriteMappedLeftovers(leftoversWithDest);

            return leftoversWithDest.Count;


        }

        private void WriteMappedLeftovers(Dictionary<(long, string), List<GenomicLocation>> leftoversWithDest)
        {
            //resetting the reader
            _leftoverReader.DiscardBufferedData();
            _leftoverReader.BaseStream.Position = 0;
            
            string line;
            while ((line = _leftoverReader.ReadLine()) != null)
            {
                var splits = line.Split('#', 3);
                var id     = long.Parse(splits[0]);
                var alt    = splits[1];
                
                if (! leftoversWithDest.ContainsKey((id, alt))) continue;
                AppendToChromFile(leftoversWithDest[(id, alt)], line);
            }
        }

        
        private void AppendToChromFile(List<GenomicLocation> leftoverLocations, string line)
        {
            foreach (GenomicLocation location in leftoverLocations)
            {
                var chromName = location.Chrom;
                if (!chromName.StartsWith("chr"))
                    chromName = "chr" + chromName;
                if (!_writers.ContainsKey(chromName))
                {
                    Console.WriteLine($"Warning!! {chromName} was not present in source but is in destination");
                    _writers.Add(chromName, GZipUtilities.GetStreamWriter(chromName +".vcf.gz"));
                }
                var splits = line.Split('\t', 3);
                _writers[chromName].WriteLine($"{chromName}\t{location.Position}\t{splits[2]}");
            }
        }
    }
}