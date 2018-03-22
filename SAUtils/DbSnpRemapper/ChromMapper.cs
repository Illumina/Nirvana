using System;
using System.Collections.Generic;
using System.IO;
using Compression.Utilities;
using VariantAnnotation.Interface.IO;

namespace SAUtils.DbSnpRemapper
{
    internal sealed class ChromMapper
    {
        private readonly StreamReader _srcReader;
        private readonly StreamReader _destReader;
        private readonly Dictionary<string, StreamWriter> _writers;
        private readonly StreamWriter _leftoverWriter;
        private int _leftoverCount;
        
        public ChromMapper(StreamReader srcReader, StreamReader destReader, StreamWriter leftoverWriter)
        {
            _srcReader  = srcReader;
            _destReader = destReader;
            _writers = new Dictionary<string, StreamWriter>();
            _leftoverWriter = leftoverWriter; 
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
                    if (!srcLine.StartsWith("#")) break;
                }
                while ((destLine= _destReader.ReadLine()) != null)
                {
                    if (!destLine.StartsWith("#")) break;
                }

                // dictionary of leftover rsIds from previous chromosomes
                
                var destRsidLocations = new Dictionary<long, int>();
                while (destLine != null && srcLine!=null)
                {
                    destRsidLocations.Clear();
                    destLine = GetNextChromDestinations(destLine, destRsidLocations);
                    srcLine = ProcessNextChromSource(srcLine, destRsidLocations);

                    //debug
                    //if (srcLine.StartsWith("chr3")) break;
                }
                
            }

            // these writers need to be kept open so that the leftover mapper can append to them
            //foreach (var writer in _writers.Values)
            //{
            //    writer.Dispose();
            //}
            Console.WriteLine($"Total leftover count:{_leftoverCount}");
            return _writers;
        }

        private string ProcessNextChromSource(string line, IDictionary<long, int> destRsidLocations)
        {
            //extracting current chrom info from first line provided
            var currentChrom = line.Split('\t', 2)[VcfCommon.ChromIndex];

            var leftoverCount=0;
            do
            {
                var splits = line.Split('\t', 4);
                var chrom = splits[VcfCommon.ChromIndex];
                if (chrom != currentChrom) break;

                var rsIds = Utilities.GetRsids(splits[VcfCommon.IdIndex]);

                if (rsIds == null) continue;

                var foundInDest = false;
                foreach (var rsId in rsIds)
                {
                    if (! destRsidLocations.TryGetValue(rsId, out var location)) continue;
                    WriteRemappedEntry(chrom, location, line);
                    destRsidLocations[rsId] = destRsidLocations[rsId] >= 0 ? -destRsidLocations[rsId] : destRsidLocations[rsId]; //flipping the sign to indicate it has been mapped
                    foundInDest = true;
                }

                if (foundInDest) continue;

                _leftoverWriter.WriteLine(line);
                leftoverCount++;

            } while ((line = _srcReader.ReadLine()) != null);

            
            Console.WriteLine($"Leftover count for {currentChrom}: {leftoverCount}");
            _leftoverCount += leftoverCount;
            return line;
        }

        private string GetNextChromDestinations(string line, IDictionary<long, int> rsIdLocations)
        {
            //extracting current chrom info from first line provided
            var currentChrom = line.Split('\t', 2)[VcfCommon.ChromIndex];
            Console.Write($"Getting destinations for chromosome:{currentChrom}...");

            do
            {
                var splits = line.Split('\t', 4);
                var chrom = splits[VcfCommon.ChromIndex];
                if (chrom != currentChrom) break;

                var position = int.Parse(splits[VcfCommon.PosIndex]);
                var rsIds = Utilities.GetRsids(splits[VcfCommon.IdIndex]);

                if (rsIds == null) continue;

                foreach (var rsId in rsIds)
                {
                   rsIdLocations.Add(rsId, position);
                }

            } while ((line = _destReader.ReadLine()) != null);

            
            Console.WriteLine($"{rsIdLocations.Count} found.");

            return line;
        }

        private void WriteRemappedEntry(string chrom, int pos, string vcfLine)
        {
            if (!_writers.ContainsKey(chrom))
                _writers[chrom] = GZipUtilities.GetStreamWriter(chrom+".vcf.gz");

            var splits = vcfLine.Split('\t', 3);

            _writers[chrom].WriteLine($"{chrom}\t{Math.Abs(pos)}\t{splits[2]}");
        }
    }
}