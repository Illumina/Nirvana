using Genome;

namespace Variants
{
    public static class VariantUtils
    {
        public const int MaxUpstreamLength = 500;

        /// <summary>
        /// Left aligns the variant using base rotation
        /// </summary>
        /// <returns>Tuple of new position, ref and alt allele</returns>
        public static (int start, string refAllele, string altAllele) TrimAndLeftAlign(int start, string refAllele, string altAllele, ISequence refSequence, int maxUpstreamLength = MaxUpstreamLength)
        {
            if (IsStructuralVariant(altAllele)) return (start, refAllele, altAllele);

            (start, refAllele, altAllele) = BiDirectionalTrimmer.Trim(start, refAllele, altAllele);

            // alignment only makes sense for insertion and deletion
            if (!(altAllele.Length == 0 || refAllele.Length == 0)) return (start, refAllele, altAllele);

            if (refSequence == null)
                return (start, refAllele, altAllele);

            //adjust the max upstream length when you are near the beginning of the chrom
            if (maxUpstreamLength >= start) maxUpstreamLength = start - 1;
            var upstreamSeq = start >= maxUpstreamLength? refSequence.Substring(start - maxUpstreamLength - 1, maxUpstreamLength):
                refSequence.Substring(0, start);
            
            // compressed seq is 0 based
            var combinedSeq = upstreamSeq;
            int repeatLength;
            int i;
            if (refAllele.Length > altAllele.Length)
            {
                // deletion
                combinedSeq += refAllele;
                repeatLength = refAllele.Length;
                for (i = combinedSeq.Length - 1; i >= repeatLength; i--, start--)
                {
                    if (combinedSeq[i] != combinedSeq[i - repeatLength]) break;
                }

                var newRefAllele = combinedSeq.Substring(i + 1 - repeatLength, repeatLength);
                return (start, newRefAllele, ""); //alt is empty for deletion
            }

            //insertion
            combinedSeq += altAllele;
            repeatLength = altAllele.Length;

            for (i = combinedSeq.Length - 1; i >= repeatLength; i--, start--)
            {
                if (combinedSeq[i] != combinedSeq[i - repeatLength]) break;
            }
            var newAltAllele = combinedSeq.Substring(i + 1 - repeatLength, repeatLength);
            return (start, "", newAltAllele);
        }

        private static bool IsStructuralVariant(string altAllele)
        {
            return altAllele.StartsWith('<') || altAllele.Contains('[') || altAllele.Contains(']');
        }
    }
}