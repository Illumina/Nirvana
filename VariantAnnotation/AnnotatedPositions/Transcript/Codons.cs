using System;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public static class Codons
    {
        public static void Assign(string transcriptReferenceAllele, string transcriptAlternateAllele,
            NullableInterval cdsInterval, NullableInterval proteinInterval, ISequence codingSequence, out string refCodons, out string altCodons)
        {
            refCodons = null;
            altCodons = null;

            if (cdsInterval.Start == null || cdsInterval.End == null || proteinInterval.Start == null ||
                proteinInterval.End == null) return;

            int aminoAcidStart = proteinInterval.Start.Value * 3 - 2;
            int aminoAcidEnd   = proteinInterval.End.Value * 3;

            int prefixLen = cdsInterval.Start.Value - aminoAcidStart;
            int suffixLen = aminoAcidEnd - cdsInterval.End.Value;

            int start1 = aminoAcidStart - 1;
            int start2 = aminoAcidEnd - suffixLen;

            int maxSuffixLen = codingSequence.Length - start2;

            if (suffixLen > maxSuffixLen) suffixLen = maxSuffixLen;

            string prefix = start1 + prefixLen < codingSequence.Length
                ? codingSequence.Substring(start1, prefixLen).ToLower()
                : "AAA";

            string suffix = suffixLen > 0
                ? codingSequence.Substring(start2, suffixLen).ToLower()
                : "";

            refCodons = GetCodon(transcriptReferenceAllele, prefix, suffix);
            altCodons = GetCodon(transcriptAlternateAllele, prefix, suffix);
        }

        /// <summary>
        /// returns the codon string consisting of the prefix and suffix bases flanking the allele bases
        /// </summary>
        internal static string GetCodon(string allele, string prefix, string suffix)
        {
            if (prefix.Length == 0 && suffix.Length == 0) return allele;
            return $"{prefix}{allele}{suffix}";
        }

        /// <summary>
        /// returns true if the length is a multiple of three, false otherwise
        /// </summary>
        public static bool IsTriplet(int len) => Math.Abs(len) % 3 == 0;


        public static void AssignExtended(string transcriptReferenceAllele, string transcriptAlternateAllele,
            NullableInterval cdsInterval, NullableInterval proteinInterval, ISequence codingSequence, out string refCodons, out string altCodons)
        {
            refCodons = null;
            altCodons = null;

            if (cdsInterval.Start == null || cdsInterval.End == null || proteinInterval.Start == null ||
                proteinInterval.End == null) return;

            int aminoAcidStart = proteinInterval.Start.Value * 3 - 2;
            int aminoAcidEnd = proteinInterval.End.Value * 3;

            int prefixLen = cdsInterval.Start.Value - aminoAcidStart;
            int suffixLen = aminoAcidEnd - cdsInterval.End.Value;

            int start1 = aminoAcidStart - 1;
            int start2 = aminoAcidEnd - suffixLen;

            int maxSuffixLen = codingSequence.Length - start2;

            var atTailEnd = false;
            if (suffixLen > maxSuffixLen)
            {
                suffixLen = maxSuffixLen;
                atTailEnd = true;
            }

            if (suffixLen > maxSuffixLen) suffixLen = maxSuffixLen;

            string prefix = start1 + prefixLen < codingSequence.Length
                ? codingSequence.Substring(start1, prefixLen).ToLower()
                : "AAA";

            string suffix = suffixLen > 0
                ? codingSequence.Substring(start2, suffixLen).ToLower()
                : "";

            var needExtend = !atTailEnd && !IsTriplet(prefixLen + suffixLen + transcriptAlternateAllele.Length);
            var extendedLen = (maxSuffixLen - suffixLen) > 45 ? 45 : (maxSuffixLen - suffixLen) / 3 * 3;
            if (needExtend) suffix = codingSequence.Substring(start2, suffixLen + extendedLen);


            refCodons = GetCodon(transcriptReferenceAllele, prefix, suffix);
            altCodons = GetCodon(transcriptAlternateAllele, prefix, suffix);
        }
    }

    
};