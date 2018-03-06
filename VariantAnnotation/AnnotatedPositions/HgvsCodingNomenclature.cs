using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.AnnotatedPositions
{
    public static class HgvsCodingNomenclature
    {
        public static string GetHgvscAnnotation(ITranscript transcript, ISimpleVariant variant, ISequence refSequence,
            int regionStart, int regionEnd)
        {
            
            // sanity check: don't try to handle odd characters, make sure this is not a reference allele, 
            //               and make sure that we have protein coordinates
            if (variant.Type == VariantType.reference || SequenceUtilities.HasNonCanonicalBase(variant.AltAllele)) return null;

            var onReverseStrand = transcript.Gene.OnReverseStrand;

            var refAllele = onReverseStrand ? SequenceUtilities.GetReverseComplement(variant.RefAllele) : variant.RefAllele;
            var altAllele = onReverseStrand ? SequenceUtilities.GetReverseComplement(variant.AltAllele) : variant.AltAllele;

            // decide event type from HGVS nomenclature
            var genomicChange = GetGenomicChange(transcript, onReverseStrand, refSequence, variant);

            var variantStart = variant.Start;
            var variantEnd   = variant.End;

            if (genomicChange == GenomicChange.Duplication)
            {
                (variantStart, variantEnd, refAllele, regionStart, regionEnd) = transcript.TranscriptRegions.ShiftDuplication(variantStart, altAllele, onReverseStrand);
            }

            var startPositionOffset = HgvsUtilities.GetCdnaPositionOffset(transcript, variantStart, regionStart);
            var endPositionOffset = variantStart == variantEnd
                ? startPositionOffset
                : HgvsUtilities.GetCdnaPositionOffset(transcript, variantEnd, regionEnd);

            if (onReverseStrand)
            {
                var tmp = startPositionOffset;
                startPositionOffset = endPositionOffset;
                endPositionOffset = tmp;
            }

            // sanity check: make sure we have coordinates
            if (startPositionOffset == null || endPositionOffset == null) return null;

            var transcriptLen = transcript.End - transcript.Start + 1;

            //_hgvs notation past the transcript
            if (startPositionOffset.Position > transcriptLen || endPositionOffset.Position > transcriptLen) return null;

            var hgvsNotation = new HgvscNotation(refAllele, altAllele, transcript.Id.WithVersion, genomicChange,
                startPositionOffset, endPositionOffset, transcript.Translation != null);

            // generic formatting
            return hgvsNotation.ToString();
        }

        /// <summary>
        /// Adjust positions by alt allele length
        /// </summary>
        internal static (int Start, int End, string RefAllele, int RegionStart, int RegionEnd) ShiftDuplication(
            this ITranscriptRegion[] regions, int start, string altAllele, bool onReverseStrand)
        {
            var incrementLength = altAllele.Length;
            var dupStart = onReverseStrand ? start + incrementLength - 1    : start - incrementLength;
            var dupEnd   = onReverseStrand ? dupStart - incrementLength + 1 : dupStart + incrementLength - 1;

            var (regionStart, _) = MappedPositionUtilities.FindRegion(regions, dupStart);
            var (regionEnd, _)   = MappedPositionUtilities.FindRegion(regions, dupEnd);

            return (dupStart, dupEnd, altAllele, regionStart, regionEnd);
        }

        public static GenomicChange GetGenomicChange(IInterval interval, bool onReverseStrand, ISequence refSequence, ISimpleVariant variant)
        {
            // length of the reference allele. Negative lengths make no sense
            var refLength = variant.End - variant.Start + 1;
            if (refLength < 0) refLength = 0;

            // length of alternative allele
            var altLength = variant.AltAllele.Length;

            // sanity check: make sure that the alleles are different
            if (variant.RefAllele == variant.AltAllele) return GenomicChange.Unknown;

            // deletion
            if (altLength == 0) return GenomicChange.Deletion;

            if (refLength == altLength)
            {
                // substitution
                if (refLength == 1) return GenomicChange.Substitution;

                // inversion
                var rcRefAllele = SequenceUtilities.GetReverseComplement(variant.RefAllele);
                return variant.AltAllele == rcRefAllele ? GenomicChange.Inversion : GenomicChange.DelIns;
            }

            // deletion/insertion
            if (refLength != 0) return GenomicChange.DelIns;

            // If this is an insertion, we should check if the preceeding reference nucleotides
            // match the insertion. In that case it should be annotated as a multiplication.
            var isGenomicDuplicate = HgvsUtilities.IsDuplicateWithinInterval(refSequence, variant, interval, onReverseStrand);

            return isGenomicDuplicate ? GenomicChange.Duplication : GenomicChange.Insertion;
        }
    }

    public enum GenomicChange
    {
        Unknown,
        Deletion,
        Duplication,
        DelIns,
        Insertion,
        Inversion,
        Substitution
    }
}
