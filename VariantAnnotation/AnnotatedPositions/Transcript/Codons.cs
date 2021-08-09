using System;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public static class Codons
    {
        private static readonly (string, string) EmptyTuple = (string.Empty, string.Empty);

        public static (string ReferenceCodons, string AlternateCodons) GetCodons(string transcriptAlternateAllele,
            int cdsStart, int cdsEnd, int proteinBegin, int proteinEnd, ReadOnlySpan<char> codingSequence)
        {
            int  cdsLength = codingSequence.Length;
            bool pastCds   = cdsStart > cdsLength && cdsEnd > cdsLength;
            if (cdsStart == -1 || cdsEnd == -1 || proteinBegin == -1 || proteinEnd == -1 || pastCds) return EmptyTuple;

            // current implementation of GetCoveredCdsAndProteinPositions may return negative cdsStart and cdsEnd beyond the CDS region
            if (cdsStart < 1) cdsStart       = 1;
            if (cdsEnd   > cdsLength) cdsEnd = cdsLength;

            int aminoAcidStart = Math.Max(proteinBegin * 3 - 2, 1);
            int aminoAcidEnd   = Math.Min(proteinEnd * 3, cdsLength);

            string transcriptReferenceAllele = cdsEnd >= cdsStart
                ? codingSequence.Slice(cdsStart - 1, cdsEnd - cdsStart + 1).ToString()
                : "";

            int prefixStartIndex = aminoAcidStart - 1;
            int prefixLen        = cdsStart       - aminoAcidStart;

            int suffixStartIndex = cdsEnd;
            int suffixLen        = aminoAcidEnd - cdsEnd;

            string prefix = prefixStartIndex + prefixLen < cdsLength
                ? codingSequence.Slice(prefixStartIndex, prefixLen).ToString().ToLower()
                : "AAA";

            string suffix = suffixLen > 0
                ? codingSequence.Slice(suffixStartIndex, suffixLen).ToString().ToLower()
                : "";

            string refCodons = GetCodon(transcriptReferenceAllele, prefix, suffix);
            string altCodons = GetCodon(transcriptAlternateAllele, prefix, suffix);
            return (refCodons, altCodons);
        }

        /// <summary>
        /// returns the codon string consisting of the prefix and suffix bases flanking the allele bases
        /// </summary>
        public static string GetCodon(string allele, string prefix, string suffix)
        {
            if (prefix.Length == 0 && suffix.Length == 0) return allele;
            return $"{prefix}{allele}{suffix}";
        }

        /// <summary>
        /// returns true if the length is a multiple of three, false otherwise
        /// </summary>
        public static bool IsTriplet(int len) => Math.Abs(len) % 3 == 0;
    }
}