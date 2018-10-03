using System;
using Genome;
using IO;
using VariantAnnotation.Interface.SA;
using Variants;

namespace VariantAnnotation.NSA
{
    public static class SuppIntervalUtilities
    {
        public static void Write(this ISuppIntervalItem item, ExtendedBinaryWriter writer)
        {
            writer.Write((byte)item.VariantType);
            writer.Write(item.GetJsonString());
        }

        public static double? GetReciprocalOverlap( IChromosomeInterval saInterval, IVariant variant)
        {
            if (saInterval.Chromosome.Index != variant.Chromosome.Index) return null;
            //skip for insertions
            if (saInterval.Start >= saInterval.End || variant.Type == VariantType.insertion) return null;
            //skip for break-ends
            if (variant.Type == VariantType.translocation_breakend) return null;

            int overlapStart = Math.Max(saInterval.Start, variant.Start);
            int overlapEnd   = Math.Min(saInterval.End, variant.End);
            int maxLen       = Math.Max(variant.End - variant.Start + 1, saInterval.End - saInterval.Start + 1);
            return Math.Max(0, (overlapEnd - overlapStart + 1) * 1.0 / maxLen);
        }
    }
}