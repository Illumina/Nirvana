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
    internal sealed class ChromMapper
    {
        private readonly StreamReader _srcReader;
        private readonly StreamReader _destReader;
        private readonly Dictionary<string, StreamWriter> _writers;
        private readonly StreamWriter _leftoverWriter;
        private readonly ISequenceProvider _srcSequenceProvider;
        private readonly ISequenceProvider _desSequenceProvider;
        private int _leftoverCount;
        private readonly Dictionary<(long, int, string), List<int>> _destinationVariants;
        private int _alleleMismatchCount;
        
        public ChromMapper(StreamReader srcReader, StreamReader destReader, StreamWriter leftoverWriter,
            ISequenceProvider srcSequenceProvider, ISequenceProvider desSequenceProvider)
        {
            _srcReader  = srcReader;
            _destReader = destReader;
            _writers = new Dictionary<string, StreamWriter>();
            _leftoverWriter = leftoverWriter;
            _srcSequenceProvider = srcSequenceProvider;
            _desSequenceProvider = desSequenceProvider;
            _destinationVariants = new Dictionary<(long, int, string), List<int>>();
        }


        public Dictionary<string, StreamWriter> Map()
        {
            using (_srcReader)
            using (_destReader)
            {
                //map all the destination rsIDs to their positions in destination

                string srcLine, destLine;
                //read to the first data line
                while ((srcLine = _srcReader.ReadLine()) != null)
                {
                    if (!srcLine.OptimizedStartsWith('#')) break;
                }
                while ((destLine= _destReader.ReadLine()) != null)
                {
                    if (!destLine.OptimizedStartsWith('#')) break;
                }

                // dictionary of leftover rsIds from previous chromosomes
                
                //var destRsidLocations = new Dictionary<long, int>();
                while (destLine != null && srcLine!=null)
                {
                    _destinationVariants.Clear();
                    destLine = GetNextChromDestinations(destLine);
                    srcLine = ProcessNextChromSource(srcLine);
                }                
            }

            // these writers need to be kept open so that the leftover mapper can append to them
            Console.WriteLine($"Total leftover count:{_leftoverCount}");
            return _writers;
        }

        private string ProcessNextChromSource(string line)
        {
            //extracting current chrom info from first line provided
            var currentChromName = line.Split('\t', 2)[VcfCommon.ChromIndex];
            var currentChrom = ReferenceNameUtilities.GetChromosome(_srcSequenceProvider.RefNameToChromosome, currentChromName);
            _srcSequenceProvider.LoadChromosome(currentChrom);
            
            var leftoverCount=0;
            do
            {
                var splits = line.Split('\t', VcfCommon.InfoIndex);
                var chrom = splits[VcfCommon.ChromIndex];
                if (chrom != currentChromName) break;
                
                var refAllele = splits[VcfCommon.RefIndex];
                var altAlleles = splits[VcfCommon.AltIndex].Split(',');
                var position = int.Parse(splits[VcfCommon.PosIndex]);
                var rsIds = Utilities.GetRsids(splits[VcfCommon.IdIndex]);
                if (rsIds == null) continue;
                
                var processedVariants = altAlleles.Select(x => VariantUtils.TrimAndLeftAlign(position, refAllele, x, _srcSequenceProvider.Sequence)).ToArray();
                
                var foundInDest = false;
                foreach (var (_, variantRef, variantAlt) in processedVariants)
                foreach (var rsId in rsIds)
                {
                    if (! _destinationVariants.TryGetValue((rsId, variantRef.Length, variantAlt), out var targetPositions)) continue;
                    
                    targetPositions.ForEach(x => WriteRemappedEntry(chrom, x, variantRef, variantAlt, line));
                    //flipping the sign to indicate it has been mapped
                    //_destinationVariants[rsId] = (-variant.position, variant.refAllele, variant.altAlleles);

                    foundInDest = true;
                }
                if (foundInDest) continue;

                foreach (var (_, _, variantAlt) in processedVariants)
                foreach (var rsId in rsIds)
                    _leftoverWriter.WriteLine(string.Join('#',rsId.ToString(), variantAlt, line));
                leftoverCount++;

            } while ((line = _srcReader.ReadLine()) != null);
            
            Console.WriteLine($"Leftover count for {currentChromName}: {leftoverCount}");
            //Console.WriteLine($"Number of entries discarded due to allele mismatch: {_alleleMismatchCount}");
            _leftoverCount += leftoverCount;
            return line;
        }

        private string GetNextChromDestinations(string line)
        {
            //extracting current chrom info from first line provided
            var currentChromName = line.Split('\t', 2)[VcfCommon.ChromIndex];
            Console.Write($"Getting destinations for chromosome:{currentChromName}...");
            var currentChrom = ReferenceNameUtilities.GetChromosome(_desSequenceProvider.RefNameToChromosome, currentChromName);
            _desSequenceProvider.LoadChromosome(currentChrom);
            do
            {
                var splits = line.Split('\t', VcfCommon.InfoIndex);
                var chrom = splits[VcfCommon.ChromIndex];
                if (chrom != currentChromName) break;

                var refAllele = splits[VcfCommon.RefIndex];
                var altAlleles = splits[VcfCommon.AltIndex].Split(',');
                var position = int.Parse(splits[VcfCommon.PosIndex]);
                var rsIds = Utilities.GetRsids(splits[VcfCommon.IdIndex]);
                if (rsIds == null) continue;

                var processedVariants = altAlleles.Select(x => VariantUtils.TrimAndLeftAlign(position, refAllele, x, _desSequenceProvider.Sequence)).ToArray();

                foreach (var (start, variantRef, variantAlt) in processedVariants)
                foreach (var rsId in rsIds)
                {
                    if (!_destinationVariants.TryGetValue((rsId, variantRef.Length, variantAlt), out var variants))
                    {
                        variants = new List<int>();
                        _destinationVariants[(rsId, variantRef.Length, variantAlt)] = variants;
                    }

                    variants.Add(start);
                }

            } while ((line = _destReader.ReadLine()) != null);

            
            Console.WriteLine($"{_destinationVariants.Count} rsIds found.");

            return line;
        }

        private void WriteRemappedEntry(string chrom, int pos, string refAllele, string altAllele, string vcfLine)
        {
            if (!_writers.ContainsKey(chrom))
                _writers[chrom] = GZipUtilities.GetStreamWriter(chrom+".vcf.gz");

            var splits = vcfLine.Split('\t', 6);

            _writers[chrom].WriteLine(string.Join('\t', chrom, pos.ToString(), splits[2], refAllele, altAllele, splits[5]));
        }
    }
}