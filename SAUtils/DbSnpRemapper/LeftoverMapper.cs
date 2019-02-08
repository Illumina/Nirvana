using System;
using System.Collections.Generic;
using System.IO;
using Compression.Utilities;
using OptimizedCore;
using VariantAnnotation.Interface.IO;

namespace SAUtils.DbSnpRemapper
{
    public sealed class LeftoverMapper
    {
        private readonly StreamReader _leftoverReader;
        private readonly StreamReader _destReader;
        private readonly Dictionary<string, StreamWriter> _writers;

        public LeftoverMapper(StreamReader leftoverReader, StreamReader destReader, Dictionary<string, StreamWriter> writers)
        {
            _leftoverReader = leftoverReader;
            _destReader = destReader;
            _writers = writers;
        }

        public int Map()
        {
            // write out the relocated locations of the leftover rsIds whenever possible
            //reading in the leftover ids
            var leftoverIds = new HashSet<long>();
            Console.Write("Loading leftover ids...");
            string line;
            while ((line = _leftoverReader.ReadLine()) != null)
            {
                var splits = line.Split('\t', 4);
                var ids = Utilities.GetRsids(splits[VcfCommon.IdIndex]);

                if (ids == null) continue;

                foreach (var id in ids)
                {
                    leftoverIds.Add(id);
                }

            }
            Console.WriteLine($"{leftoverIds.Count} found.");

            // stream through the dest file to find locations
            var leftoversWithDest = new Dictionary<long, GenomicLocation>();
            while ((line = _destReader.ReadLine()) != null)
            {
                if (line.OptimizedStartsWith('#')) continue;
                var splits = line.Split('\t', 4);
                var ids = Utilities.GetRsids(splits[VcfCommon.IdIndex]);

                if (ids == null) continue;

                foreach (var id in ids)
                {
                    if (!leftoverIds.Contains(id)) continue;
                    var chrom = splits[VcfCommon.ChromIndex];
                    var pos = int.Parse(splits[VcfCommon.PosIndex]);
                    leftoversWithDest.Add(id, new GenomicLocation(chrom, pos));
                }

            }

            WriteMappedLeftovers(leftoversWithDest);

            return leftoversWithDest.Count;


        }

        private void WriteMappedLeftovers(IReadOnlyDictionary<long, GenomicLocation> leftoversWithDest)
        {
            //resetting the reader
            _leftoverReader.DiscardBufferedData();
            _leftoverReader.BaseStream.Position = 0;
            
            string line;
            while ((line = _leftoverReader.ReadLine()) != null)
            {
                var splits = line.Split('\t', 4);
                var ids = Utilities.GetRsids(splits[VcfCommon.IdIndex]);

                if (ids == null) continue;

                foreach (var id in ids)
                {
                    if (! leftoversWithDest.ContainsKey(id)) continue;
                    AppendToChromFile(leftoversWithDest[id], line);
                }
            }
        }

        
        private void AppendToChromFile(GenomicLocation leftoverLocation, string line)
        {
            var chromName = leftoverLocation.Chrom;
            if (!chromName.StartsWith("chr"))
                chromName = "chr" + chromName;
            if (!_writers.ContainsKey(chromName))
            {
                Console.WriteLine($"Warning!! {chromName} was not present in source but is in destination");
                _writers.Add(chromName, GZipUtilities.GetStreamWriter(chromName+".vcf.gz"));
            }

            var splits = line.Split('\t', 3);

            _writers[chromName].WriteLine($"{chromName}\t{leftoverLocation.Position}\t{splits[2]}");
            
        }
    }
}