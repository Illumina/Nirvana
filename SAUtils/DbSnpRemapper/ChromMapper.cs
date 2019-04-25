using System;
using System.Collections.Generic;
using System.IO;
using Compression.Utilities;
using OptimizedCore;
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
        private readonly Dictionary<long, (int position, string refAllele, string[] altAlleles)> _destinationVariants;
        private int _alleleMismatchCount;
        
        public ChromMapper(StreamReader srcReader, StreamReader destReader, StreamWriter leftoverWriter)
        {
            _srcReader  = srcReader;
            _destReader = destReader;
            _writers = new Dictionary<string, StreamWriter>();
            _leftoverWriter = leftoverWriter; 
            _destinationVariants = new Dictionary<long, (int position, string refAllele, string[] altAlleles)>();
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
            var currentChrom = line.Split('\t', 2)[VcfCommon.ChromIndex];

            var leftoverCount=0;
            do
            {
                var splits = line.Split('\t', VcfCommon.InfoIndex);
                var chrom = splits[VcfCommon.ChromIndex];
                if (chrom != currentChrom) break;
                var refAllele = splits[VcfCommon.RefIndex];
                var altAlleles = splits[VcfCommon.AltIndex].Split(',');
                
                var rsIds = Utilities.GetRsids(splits[VcfCommon.IdIndex]);

                if (rsIds == null) continue;
                
                var foundInDest = false;
                var hasAlleleMismatch = false;
                foreach (var rsId in rsIds)
                {
                    if (! _destinationVariants.TryGetValue(rsId, out var variant)) continue;
                    if (//variant.position < 0 || 
                        refAllele != variant.refAllele ||
                        ! Utilities.HasCommonAlleles(altAlleles, variant.altAlleles))
                    {
                        _alleleMismatchCount++;
                        hasAlleleMismatch = true;
                        continue;
                    }

                    WriteRemappedEntry(chrom, variant.position, line);
                    //flipping the sign to indicate it has been mapped
                    //_destinationVariants[rsId] = (-variant.position, variant.refAllele, variant.altAlleles);

                    foundInDest = true;
                }

                if (foundInDest || hasAlleleMismatch) continue;

                _leftoverWriter.WriteLine(line);
                leftoverCount++;

            } while ((line = _srcReader.ReadLine()) != null);

            
            Console.WriteLine($"Leftover count for {currentChrom}: {leftoverCount}");
            Console.WriteLine($"Number of entries discarded due to allele mismatch: {_alleleMismatchCount}");
            _leftoverCount += leftoverCount;
            return line;
        }

        private string GetNextChromDestinations(string line)
        {
            //extracting current chrom info from first line provided
            var currentChrom = line.Split('\t', 2)[VcfCommon.ChromIndex];
            Console.Write($"Getting destinations for chromosome:{currentChrom}...");

            do
            {
                var splits = line.Split('\t', VcfCommon.InfoIndex);
                var chrom = splits[VcfCommon.ChromIndex];
                if (chrom != currentChrom) break;

                var refAllele = splits[VcfCommon.RefIndex];
                var altAlleles = splits[VcfCommon.AltIndex].Split(',');
                var position = int.Parse(splits[VcfCommon.PosIndex]);
                var rsIds = Utilities.GetRsids(splits[VcfCommon.IdIndex]);

                if (rsIds == null) continue;

                foreach (var rsId in rsIds)
                {
                   _destinationVariants.Add(rsId, (position, refAllele, altAlleles));
                }

            } while ((line = _destReader.ReadLine()) != null);

            
            Console.WriteLine($"{_destinationVariants.Count} rsIds found.");

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