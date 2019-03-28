using System;
using Genome;
using Variants;
using static Intervals.Utilities;

namespace VariantAnnotation.NSA
{
    public static class SuppIntervalUtilities
    {
        public static (double? ReciprocalOverlap, double? AnnotationOverlap) GetOverlapFractions( IChromosomeInterval saInterval, ISimpleVariant variant)
        {
            if (saInterval.Chromosome.Index != variant.Chromosome.Index) return (null, null);
            //skip for insertions
            if (saInterval.Start >= saInterval.End || variant.Type == VariantType.insertion) return (null, null);
            //skip for break-ends
            if (variant.Type == VariantType.translocation_breakend) return (null, null);

            if (!Overlaps(saInterval.Start, saInterval.End, variant.Start, variant.End)) return (null, null);

            var overlapSize = (double) (Math.Min(saInterval.End, variant.End) - Math.Max(saInterval.Start, variant.Start) + 1);
            int annoSize = saInterval.End - saInterval.Start + 1;
            int varSize = variant.End - variant.Start + 1;
            int maxSize = Math.Max(annoSize, varSize);
            return (overlapSize / maxSize, overlapSize / annoSize);
        }
    }
}