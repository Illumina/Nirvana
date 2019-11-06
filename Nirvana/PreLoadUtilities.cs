using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Utilities;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using Variants;

namespace Nirvana
{
    public static class PreLoadUtilities
    {
        public static IDictionary<IChromosome, List<int>> GetPositions(Stream vcfStream, GenomicRange genomicRange, ISequenceProvider sequenceProvider, IRefMinorProvider refMinorProvider)
        {
            var benchmark = new Benchmark();
            Console.Write("Scanning positions required for SA pre-loading....");
            var chromPositions = new Dictionary<IChromosome, List<int>>();
            var rangeChecker = new GenomicRangeChecker(genomicRange);
            var refNameToChrom = sequenceProvider.RefNameToChromosome;

            using (var reader = new StreamReader(vcfStream))
            {
                string line;
                string currentChromName="";
                IChromosome chrom = null;
                while ((line = reader.ReadLine()) != null)
                {
                    if(line.StartsWith("#")) continue; //skip header lines
                    
                    var splits = line.OptimizedSplit('\t');
                    string chromName = splits[VcfCommon.ChromIndex];
                    if (chromName != currentChromName)
                    {
                        if (!refNameToChrom.TryGetValue(chromName, out chrom)) continue;//skip unrecognized contigs
                        currentChromName = chromName;
                    }

                    (int position, bool foundError) = splits[VcfCommon.PosIndex].OptimizedParseInt32();
                    if (foundError) throw new InvalidDataException($"Unable to convert the VCF position to an integer: {splits[VcfCommon.PosIndex]}");

                    if (rangeChecker.OutOfRange(chrom, position)) break;

                    string refAllele = splits[VcfCommon.RefIndex];
                    string altAllele = splits[VcfCommon.AltIndex];
                    //skip ref positions unless ref minor
                    // for ref positions altAllele=='.'
                    if(altAllele == "." && !IsRefMinor(refMinorProvider, chrom, position))
                        continue;

                    sequenceProvider.LoadChromosome(chrom);
                    TryAddPosition(chromPositions, chrom, position, refAllele, altAllele, sequenceProvider.Sequence);
                }
            }

            int count = SortPositionsAndGetCount(chromPositions);

            Console.WriteLine($"{count} positions found in {Benchmark.ToHumanReadable(benchmark.GetElapsedTime())}");

            return chromPositions;
        }

        private static bool IsRefMinor(IRefMinorProvider refMinorProvider, IChromosome chrom, int position)
        {
            if (refMinorProvider == null) return false;
            return !string.IsNullOrEmpty(refMinorProvider.GetGlobalMajorAllele(chrom, position));
        }

        public static bool TryAddPosition(Dictionary<IChromosome, List<int>> chromPositions, IChromosome chromosome, int position, string refAllele, string altAllele, ISequence refSequence)
        {
            if (!chromPositions.ContainsKey(chromosome)) chromPositions.Add(chromosome, new List<int>(16 * 1024));
            var addedPosition = false;
            foreach (string allele in altAllele.OptimizedSplit(','))
            {
                if (allele.OptimizedStartsWith('<') || allele.Contains('[') || altAllele.Contains(']')) continue;

                (int shiftedPos, string _, string _) =
                    VariantUtils.TrimAndLeftAlign(position, refAllele, allele, refSequence);
                chromPositions[chromosome].Add(shiftedPos);
                addedPosition = true;
            }

            return addedPosition;
        }

        private static int SortPositionsAndGetCount(Dictionary<IChromosome, List<int>> chromPositions)
        {
            var count = 0;
            foreach (var positions in chromPositions.Values)
            {
                positions.Sort();
                count += positions.Count;
            }

            return count;
        }
    }
}