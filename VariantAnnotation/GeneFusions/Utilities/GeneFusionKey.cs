using System;

namespace VariantAnnotation.GeneFusions.Utilities
{
    public static class GeneFusionKey
    {
        public static ulong Create(string geneId, string geneId2)
        {
            // if we're missing an Ensembl Gene ID, return a zero
            // this is guaranteed not to match anything in a gene fusion source SA file
            if (geneId == null || geneId2 == null) return 0;
            
            ReadOnlySpan<char> geneSpan  = geneId.AsSpan().Slice(4);
            ReadOnlySpan<char> gene2Span = geneId2.AsSpan().Slice(4);

            uint num  = uint.Parse(geneSpan);
            uint num2 = uint.Parse(gene2Span);

            (ulong min, ulong max) = num < num2 ? (num, num2) : (num2, num);
            return min << 32 | max;
        }
    }
}