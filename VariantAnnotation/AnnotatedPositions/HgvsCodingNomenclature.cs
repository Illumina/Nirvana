using System.IO;
using Cache.Data;
using Genome;
using Intervals;
using Variants;

namespace VariantAnnotation.AnnotatedPositions
{
    public static class HgvsCodingNomenclature
    {
        public static string GetHgvscAnnotation(Transcript transcript, ISimpleVariant variant, ISequence refSequence,
            int regionStart, int regionEnd, string transcriptRef, string transcriptAlt)
        {
            // sanity check: don't try to handle odd characters, make sure this is not a reference allele, 
            //               and make sure that we have protein coordinates
            if (variant.Type == VariantType.reference ||
                SequenceUtilities.HasNonCanonicalBase(variant.AltAllele)) return null;

            // do not report HGVSc notation when variant lands inside gap region
            bool onReverseStrand = transcript.Gene.OnReverseStrand;

            if (IsVariantInRnaEditDeletion(variant, onReverseStrand, regionStart, regionEnd,
                    transcript.TranscriptRegions)) return null;

            string refAllele = string.IsNullOrEmpty(transcriptRef)
                ? onReverseStrand ? SequenceUtilities.GetReverseComplement(variant.RefAllele) : variant.RefAllele
                : transcriptRef;
            string altAllele = string.IsNullOrEmpty(transcriptAlt)
                ? onReverseStrand ? SequenceUtilities.GetReverseComplement(variant.AltAllele) : variant.AltAllele
                : transcriptAlt;

            // decide event type from HGVS nomenclature
            var genomicChange = GetGenomicChange(transcript, onReverseStrand, refSequence, variant);

            int variantStart = variant.Start;
            int variantEnd   = variant.End;

            if (genomicChange == GenomicChange.Duplication)
            {
                (variantStart, variantEnd, refAllele, regionStart, regionEnd) =
                    transcript.TranscriptRegions.ShiftDuplication(variantStart, altAllele, onReverseStrand);
            }

            var startPositionOffset = HgvsUtilities.GetPositionOffset(transcript, variantStart, regionStart);
            var endPositionOffset = variantStart == variantEnd
                ? startPositionOffset
                : HgvsUtilities.GetPositionOffset(transcript, variantEnd, regionEnd);

            if (onReverseStrand)
            {
                (startPositionOffset, endPositionOffset) = (endPositionOffset, startPositionOffset);
            }

            if (startPositionOffset == null && variant.Type == VariantType.insertion)
            {
                startPositionOffset = new PositionOffset(endPositionOffset.Position + 1, endPositionOffset.Offset,
                    $"{endPositionOffset.Position + 1}", endPositionOffset.HasStopCodonNotation);
            }

            // sanity check: make sure we have coordinates
            if (startPositionOffset == null || endPositionOffset == null) return null;

            var hgvsNotation = new HgvscNotation(refAllele, altAllele, transcript.Id, genomicChange,
                startPositionOffset, endPositionOffset, transcript.CodingRegion != null);

            // generic formatting
            return hgvsNotation.ToString();
        }

        private static bool IsVariantInRnaEditDeletion(IInterval variant, bool onReverseStrand, int regionStart,
            int regionEnd, TranscriptRegion[] transcriptRegions)
        {
            if (regionStart == -1 || regionEnd == -1 || regionStart != regionEnd) return false;

            var region = transcriptRegions[regionStart];
            if (region.CigarOps == null) return false;

            CigarOp startOp = FindCigarOp(variant.Start, region, onReverseStrand);
            CigarOp endOp   = FindCigarOp(variant.End,   region, onReverseStrand);

            return startOp == endOp && startOp.Type == CigarType.Deletion;
        }

        private static CigarOp FindCigarOp(int position, TranscriptRegion region, bool onReverseStrand)
        {
            int remainingBases = (onReverseStrand ? region.End - position : position - region.Start) + 1;

            foreach (var cigarOp in region.CigarOps!)
            {
                bool lastCigarOp = cigarOp.Length >= remainingBases;
                if (lastCigarOp) return cigarOp;
                remainingBases -= cigarOp.Length;
            }

            throw new InvalidDataException(
                $"Unable to find CigarOp for position {position} in region {region.Start}-{region.End}");
        }

        internal static (int Start, int End, string RefAllele, int RegionStart, int RegionEnd) ShiftDuplication(
            this TranscriptRegion[] regions, int start, string altAllele, bool onReverseStrand)
        {
            int incrementLength = altAllele.Length;
            int dupStart        = onReverseStrand ? start    + incrementLength - 1 : start - incrementLength;
            int dupEnd          = onReverseStrand ? dupStart - incrementLength + 1 : dupStart + incrementLength - 1;

            (int regionStart, _) = MappedPositionUtilities.FindRegion(regions, dupStart);
            (int regionEnd, _)   = MappedPositionUtilities.FindRegion(regions, dupEnd);

            return (dupStart, dupEnd, altAllele, regionStart, regionEnd);
        }

        public static GenomicChange GetGenomicChange(IInterval interval, bool onReverseStrand, ISequence refSequence,
            ISimpleVariant variant)
        {
            // length of the reference allele. Negative lengths make no sense
            int refLength = variant.End - variant.Start + 1;
            if (refLength < 0) refLength = 0;

            // length of alternative allele
            int altLength = variant.AltAllele.Length;

            // sanity check: make sure that the alleles are different
            if (variant.RefAllele == variant.AltAllele) return GenomicChange.Unknown;

            // deletion
            if (altLength == 0) return GenomicChange.Deletion;

            if (refLength == altLength)
            {
                // substitution
                if (refLength == 1) return GenomicChange.Substitution;

                // inversion
                string rcRefAllele = SequenceUtilities.GetReverseComplement(variant.RefAllele);
                return variant.AltAllele == rcRefAllele ? GenomicChange.Inversion : GenomicChange.DelIns;
            }

            // deletion/insertion
            if (refLength != 0) return GenomicChange.DelIns;

            // If this is an insertion, we should check if the preceding reference nucleotides
            // match the insertion. In that case it should be annotated as a multiplication.
            bool isGenomicDuplicate =
                HgvsUtilities.IsDuplicateWithinInterval(refSequence, variant, interval, onReverseStrand);

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