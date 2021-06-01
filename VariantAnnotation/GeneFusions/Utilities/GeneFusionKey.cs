using System;

namespace VariantAnnotation.GeneFusions.Utilities
{
    public static class GeneFusionKey
    {
        public static ulong Create(uint num, uint num2)
        {
            if (num == 0 || num2 == 0) return 0;
            (ulong min, ulong max) = num < num2 ? (num, num2) : (num2, num);
            return min << 32 | max;
        }

        public static uint CreateGeneKey(string geneId)
        {
            if (geneId == null) return 0;
            ReadOnlySpan<char> geneSpan = geneId.AsSpan().Slice(4);
            return uint.Parse(geneSpan);
        }
    }
}