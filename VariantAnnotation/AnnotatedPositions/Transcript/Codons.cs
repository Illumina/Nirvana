using System;
using Genome;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public static class Codons
    {
        public static (string Reference, string Alternate) GetCodons(string transcriptAlternateAllele,
            int cdsStart, int cdsEnd, int proteinBegin, int proteinEnd, ISequence codingSequence)
        {
            if (cdsStart == -1 || cdsEnd == -1 || proteinBegin == -1 || proteinEnd == -1) return ("", "");

            var transcriptReferenceAllele = cdsEnd >= cdsStart ? codingSequence.Substring(cdsStart - 1, cdsEnd - cdsStart + 1) : "";

            int aminoAcidStart = proteinBegin * 3 - 2;
            int aminoAcidEnd   = proteinEnd * 3;

            int prefixStartIndex = aminoAcidStart - 1;
            int prefixLen = cdsStart - aminoAcidStart;

            int suffixStartIndex = cdsEnd;
            int suffixLen = Math.Min(aminoAcidEnd, codingSequence.Length) - cdsEnd;

            string prefix = prefixStartIndex + prefixLen < codingSequence.Length
                ? codingSequence.Substring(prefixStartIndex, prefixLen).ToLower()
                : "AAA";

            string suffix = suffixLen > 0
                ? codingSequence.Substring(suffixStartIndex, suffixLen).ToLower()
                : "";

            var refCodons = GetCodon(transcriptReferenceAllele, prefix, suffix);
            var altCodons = GetCodon(transcriptAlternateAllele, prefix, suffix);
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