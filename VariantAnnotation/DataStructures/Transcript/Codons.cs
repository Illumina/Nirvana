using System;
using VariantAnnotation.DataStructures.Annotation;
using VariantAnnotation.DataStructures.CompressedSequence;

namespace VariantAnnotation.DataStructures.Transcript
{
    public static class Codons
    {
        /// <summary>
        /// assigns the reference and alternate codons [TranscriptVariationAllele.pm:259 codon]
        /// </summary>
        public static void Assign(TranscriptAnnotation ta, Transcript transcript, ICompressedSequence compressedSequence)
        {
            // sanity check: make sure this is a coding region
            if (!(ta.HasValidCdsEnd && ta.HasValidCdsStart))
            {
                ta.ReferenceCodon = null;
                ta.AlternateCodon = null;
                return;
            }

            // calculate necessary coordinates and lengths
            int aminoAcidStart = ta.ProteinBegin * 3 - 2;
            int aminoAcidEnd   = ta.ProteinEnd * 3;

            int prefixLen = ta.CodingDnaSequenceBegin - aminoAcidStart;
            int suffixLen = aminoAcidEnd - ta.CodingDnaSequenceEnd;

            var codingSequence = new CodingSequence(compressedSequence, transcript.Translation.CodingRegion.GenomicStart,
                transcript.Translation.CodingRegion.GenomicEnd, transcript.CdnaMaps, transcript.Gene.OnReverseStrand,
                transcript.StartExonPhase);
            var aminoAcidSeq = codingSequence.Sequence();

            int start1 = aminoAcidStart - 1;
            int start2 = aminoAcidEnd - suffixLen;

            int maxSuffixLen = aminoAcidSeq.Length - start2;

            bool atTailEnd = false;
            if (suffixLen > maxSuffixLen)
            {
                suffixLen = maxSuffixLen;
                atTailEnd = true;
            }

            if (start1    < 0) start1    = 0;
            if (start2    < 0) start2    = 0;
            if (prefixLen < 0) prefixLen = 0;

            string prefix = start1 + prefixLen < aminoAcidSeq.Length
                ? aminoAcidSeq.Substring(start1, prefixLen).ToLower()
                : "AAA";

            string suffix = suffixLen > 0
                ? aminoAcidSeq.Substring(start2, suffixLen).ToLower()
                : "";

            ta.HasFrameShift  = false;
            ta.ReferenceCodon = GetCodon(ta.TranscriptReferenceAllele, prefix, suffix, ref ta.HasFrameShift, atTailEnd);
            ta.AlternateCodon = GetCodon(ta.TranscriptAlternateAllele, prefix, suffix, ref ta.HasFrameShift, atTailEnd);
        }

		/// <summary>
		/// assigns the reference and alternate codons [TranscriptVariationAllele.pm:259 codon]
		/// if frameshift variants, add 45 basepair for both ref and alt codon
		/// added 45 base pair for stop loss variant
		/// </summary>
		public static void AssignExtended(TranscriptAnnotation ta, Transcript transcript, ICompressedSequence compressedSequence)
		{
			// sanity check: make sure this is a coding region
			if (!(ta.HasValidCdsEnd && ta.HasValidCdsStart))
			{
				ta.ReferenceCodon = null;
				ta.AlternateCodon = null;
				return;
			}

			// calculate necessary coordinates and lengths
			var aminoAcidStart = ta.ProteinBegin * 3 - 2;
			var aminoAcidEnd = ta.ProteinEnd * 3;

			var prefixLen = ta.CodingDnaSequenceBegin - aminoAcidStart;
			var suffixLen = aminoAcidEnd - ta.CodingDnaSequenceEnd;

		    var codingSequence = new CodingSequence(compressedSequence, transcript.Translation.CodingRegion.GenomicStart,
		        transcript.Translation.CodingRegion.GenomicEnd, transcript.CdnaMaps, transcript.Gene.OnReverseStrand,
		        transcript.StartExonPhase);
            var aminoAcidSeq = codingSequence.Sequence();

            var start1 = aminoAcidStart - 1;
			var start2 = aminoAcidEnd - suffixLen;

			var maxSuffixLen = aminoAcidSeq.Length - start2;

			var atTailEnd = false;
			if (suffixLen > maxSuffixLen)
			{
				suffixLen = maxSuffixLen;
				atTailEnd = true;
			}

			if (start1 < 0) start1 = 0;
			if (start2 < 0) start2 = 0;
			if (prefixLen < 0) prefixLen = 0;

			var prefix = start1 + prefixLen < aminoAcidSeq.Length
				? aminoAcidSeq.Substring(start1, prefixLen).ToLower()
				: "AAA";

			var suffix = suffixLen > 0
				? aminoAcidSeq.Substring(start2, suffixLen).ToLower()
				: "";

			var needExtend = !atTailEnd && !IsTriplet(prefixLen + suffixLen + ta.TranscriptAlternateAllele.Length);

			var extendedLen = maxSuffixLen - suffixLen > 45 ? 45 : (maxSuffixLen - suffixLen)/3*3;

			if (needExtend) suffix = aminoAcidSeq.Substring(start2, suffixLen + extendedLen);

			ta.HasFrameShift = false;
			ta.ReferenceCodon = GetCodon(ta.TranscriptReferenceAllele, prefix, suffix, ref ta.HasFrameShift, atTailEnd);
			ta.AlternateCodon = GetCodon(ta.TranscriptAlternateAllele, prefix, suffix, ref ta.HasFrameShift, atTailEnd);
		}

		/// <summary>
		/// returns the codon string consisting of the prefix and suffix bases flanking the allele bases
		/// </summary>
		private static string GetCodon(string allele, string prefix, string suffix, ref bool hasFrameShift,
            bool atTailEnd)
        {
            int alleleLen = allele.Length;
            int prefixLen = prefix.Length;
            int suffixLen = suffix.Length;

            // sanity check: handle frameshift variations
            // if we are at the tail end, we don't need to have a triplet
            if (!atTailEnd && !IsTriplet(prefixLen + suffixLen + alleleLen)) hasFrameShift = true;

            // sanity check: nothing to do
            if (prefixLen == 0 && suffixLen == 0) return alleleLen == 0 ? string.Empty : allele;

            // concatenate the extra bases to our alleles
            return $"{prefix}{allele}{suffix}";
        }

        /// <summary>
        /// returns true if the length is a multiple of three, false otherwise
        /// </summary>
        public static bool IsTriplet(int len)
        {
            return Math.Abs(len) % 3 == 0;
        }
    }
}