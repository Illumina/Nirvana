using System;
using System.Collections.Generic;
using CommandLine.Utilities;
using Compression.Utilities;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.IO;
using Variants;

namespace Nirvana
{
    public static class PreLoadUtilities
    {
        public static Dictionary<IChromosome, List<int>> GetPositions(string vcfFile, IDictionary<string, IChromosome> refNameToChrom)
        {
            var benchmark = new Benchmark();
            Console.Write("Preloading variant positions....");
            var chromPositions = new Dictionary<IChromosome, List<int>>();
            using (var reader = GZipUtilities.GetAppropriateStreamReader(vcfFile))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith('#')) continue;
                    var splits = line.Split('\t', 6);

                    string chrom     = splits[VcfCommon.ChromIndex];
                    if (!refNameToChrom.TryGetValue(chrom, out var iChrom)) continue;

                    int position     = int.Parse(splits[VcfCommon.PosIndex]);
                    string refAllele = splits[VcfCommon.RefIndex];
                    string altAllele = splits[VcfCommon.AltIndex];

                    foreach (string allele in altAllele.OptimizedSplit(','))
                    {
                        if (allele.OptimizedStartsWith('<')|| allele.Equals(".")) continue;
                        
                        (int trimPos, string _, string _) = BiDirectionalTrimmer.Trim(position, refAllele,allele);
                        
                        if (chromPositions.TryGetValue(iChrom, out var positionList))
                            positionList.Add(trimPos);
                        else
                            chromPositions.Add(iChrom, new List<int>(16*1024){trimPos});
                        
                    }
                }
            }

            var count = 0;
            foreach (var positions in chromPositions.Values)
            {
                positions.Sort();
                count += positions.Count;
            }

            Console.WriteLine($"{count} positions found in {Benchmark.ToHumanReadable(benchmark.GetElapsedTime())}");

            return chromPositions;
        }
    }
}