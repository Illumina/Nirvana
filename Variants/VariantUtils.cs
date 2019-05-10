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
            //we have to check this before the trimming since it depends on the padding base
            bool isLeftShiftPossible = IsLeftShiftPossible(refAllele, altAllele);

            (start, refAllele, altAllele) = BiDirectionalTrimmer.Trim(start, refAllele, altAllele);

            // alignment only makes sense for insertion and deletion
            if (!(altAllele.Length == 0 || refAllele.Length == 0)) return (start, refAllele, altAllele);
            if(! isLeftShiftPossible) return (start, refAllele, altAllele);

            //base checking to make sure we can safely left shift
            if (IfRefBaseMismatched(start, refAllele, refSequence)) return (start, refAllele, altAllele);

            //adjust the max upstream length when you are near the beginning of the chrom
            if (maxUpstreamLength >= start) maxUpstreamLength = start - 1;
            var upstreamSeq = refSequence.Substring(start - maxUpstreamLength - 1, maxUpstreamLength);
            
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

        private static bool IfRefBaseMismatched(int start, string refAllele, ISequence refSequence)
        {
            return refSequence != null && !string.IsNullOrEmpty(refAllele) && refAllele != refSequence.Substring(start - 1, refAllele.Length);
        }

        // we have a padding base we can check if its possible to left shift at all
        public static bool IsLeftShiftPossible(string refAllele, string altAllele)
        {
            if (refAllele == altAllele) return false;
            if (string.IsNullOrEmpty(refAllele) || string.IsNullOrEmpty(altAllele)) return true;
            if (refAllele.Length == 1) return refAllele[0] == altAllele[altAllele.Length - 1];
            if (altAllele.Length == 1) return altAllele[0] == refAllele[refAllele.Length - 1];

            return true;
        }

        private static bool IsStructuralVariant(string altAllele)
        {
            return altAllele.StartsWith('<') || altAllele.Contains('[') || altAllele.Contains(']');
        }
    }
}