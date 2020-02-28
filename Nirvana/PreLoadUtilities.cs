using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using Variants;

namespace Nirvana
{
    public static class PreLoadUtilities
    {
        public static (ImmutableDictionary<IChromosome, List<int>> PositionsByChromosome, int Count) GetPositions(Stream vcfStream, GenomicRange genomicRange,
            ISequenceProvider sequenceProvider, IRefMinorProvider refMinorProvider)
        {
            var positionsByChromosome = new Dictionary<IChromosome, List<int>>();
            var rangeChecker          = new GenomicRangeChecker(genomicRange);
            var refNameToChrom        = sequenceProvider.RefNameToChromosome;

            using (var reader = new StreamReader(vcfStream))
            {
                string      line;
                string      currentReferenceName = "";
                IChromosome chromosome           = null;
                
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith('#')) continue;

                    string[] cols          = line.OptimizedSplit('\t');
                    string   referenceName = cols[VcfCommon.ChromIndex];
                    
                    if (referenceName != currentReferenceName)
                    {
                        if (!refNameToChrom.TryGetValue(referenceName, out chromosome)) continue;
                        currentReferenceName = referenceName;
                    }

                    (int position, bool foundError) = cols[VcfCommon.PosIndex].OptimizedParseInt32();
                    if (foundError) throw new InvalidDataException($"Unable to convert the VCF position to an integer: {cols[VcfCommon.PosIndex]}");

                    if (rangeChecker.OutOfRange(chromosome, position)) break;

                    string refAllele = cols[VcfCommon.RefIndex];
                    string altAllele = cols[VcfCommon.AltIndex];

                    if (altAllele == "." && !IsRefMinor(refMinorProvider, chromosome, position)) continue;

                    sequenceProvider.LoadChromosome(chromosome);
                    TryAddPosition(positionsByChromosome, chromosome, position, refAllele, altAllele, sequenceProvider.Sequence);
                }
            }
            
            int count = SortPositionsAndGetCount(positionsByChromosome);

            return (positionsByChromosome.ToImmutableDictionary(), count);
        }

        private static bool IsRefMinor(IRefMinorProvider refMinorProvider, IChromosome chrom, int position)
        {
            if (refMinorProvider == null) return false;
            return !string.IsNullOrEmpty(refMinorProvider.GetGlobalMajorAllele(chrom, position));
        }

        public static void TryAddPosition(Dictionary<IChromosome, List<int>> chromPositions, IChromosome chromosome,
            int position, string refAllele, string altAllele, ISequence refSequence)
        {
            if (!chromPositions.ContainsKey(chromosome)) chromPositions.Add(chromosome, new List<int>(16 * 1024));

            foreach (string allele in altAllele.OptimizedSplit(','))
            {
                if (allele.OptimizedStartsWith('<') && allele != "<NON_REF>" || allele.Contains('[') || altAllele.Contains(']')) continue;

                (int shiftedPos, string _, string _) =
                    VariantUtils.TrimAndLeftAlign(position, refAllele, allele, refSequence);
                chromPositions[chromosome].Add(shiftedPos);
            }
        }

        private static int SortPositionsAndGetCount(Dictionary<IChromosome, List<int>> positionsByChromosome)
        {
            var count = 0;

            foreach (var positions in positionsByChromosome.Values)
            {
                positions.Sort();
                count += positions.Count;
            }

            return count;
        }
    }
}