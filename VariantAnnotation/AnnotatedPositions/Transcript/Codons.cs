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

            // current implementation of GetCoveredCdsAndProteinPositions may return negative cdsStart and cdsEnd beyond the CDS region
            if (cdsStart < 1) cdsStart = 1;
            if (cdsEnd > codingSequence.Length) cdsEnd = codingSequence.Length;

            int aminoAcidStart = Math.Max(proteinBegin * 3 - 2, 1);
            int aminoAcidEnd = Math.Min(proteinEnd * 3, codingSequence.Length);

            var transcriptReferenceAllele = cdsEnd >= cdsStart ? codingSequence.Substring(cdsStart - 1, cdsEnd - cdsStart + 1) : "";

            int prefixStartIndex = aminoAcidStart - 1;
            int prefixLen = cdsStart - aminoAcidStart;

            int suffixStartIndex = cdsEnd;
            int suffixLen = aminoAcidEnd - cdsEnd;

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