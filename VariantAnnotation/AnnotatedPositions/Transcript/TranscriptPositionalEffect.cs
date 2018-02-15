using System.Linq;
using VariantAnnotation.Algorithms;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class TranscriptPositionalEffect
    {
        public bool IsEndSpliceSite;
        public bool IsStartSpliceSite;
        public bool IsWithinFrameshiftIntron;
        public bool IsWithinIntron;
        public bool IsWithinSpliceSiteRegion;

        public bool HasExonOverlap;
        public bool AfterCoding;
        public bool BeforeCoding;
        public bool WithinCdna;
        public bool WithinCds;
        public bool HasFrameShift;
        public bool IsCoding;

        public bool OverlapWithMicroRna;

        public void DetermineIntronicEffect(ITranscriptRegion[] regions, IInterval variant, VariantType variantType)
        {
            if (regions == null) return;

            var isInsertion = variantType == VariantType.insertion;

            foreach (var region in regions)
            {
                if (region.Type != TranscriptRegionType.Intron) continue;

                // TODO: we should sort our introns so that we can end early

                // skip this one if variant is out of range : the range is set to 3 instead of the original old:
                // all of the checking occured in the region between start-3 to end+3, if we set to 8, we can made mistakes when
                // checking IsWithinIntron when we have a small exon
                if (!variant.Overlaps(region.Start - 3, region.End + 3)) continue;

                // under various circumstances the genebuild process can introduce artificial 
                // short (<= 12 nucleotide) introns into transcripts (e.g. to deal with errors
                // in the reference sequence etc.), we don't want to categorize variations that
                // fall in these introns as intronic, or as any kind of splice variant

                var isFrameshiftIntron = region.End - region.Start <= 12;

                if (isFrameshiftIntron)
                {
                    if (variant.Overlaps(region.Start, region.End))
                    {
                        IsWithinFrameshiftIntron = true;
                        continue;
                    }
                }

                if (variant.Overlaps(region.Start, region.Start + 1))
                {
                    IsStartSpliceSite = true;
                }

                if (variant.Overlaps(region.End - 1, region.End))
                {
                    IsEndSpliceSite = true;
                }

                // we need to special case insertions between the donor and acceptor sites

                //make sure the size of intron is larger than 4
                if (region.Start <= region.End - 4)
                {
                    if (variant.Overlaps(region.Start + 2, region.End - 2) ||
                        isInsertion && (variant.Start == region.Start + 2
                                        || variant.End == region.End - 2))
                    {
                        IsWithinIntron = true;
                    }
                }

                // the definition of splice_region (SO:0001630) is "within 1-3 bases of the
                // exon or 3-8 bases of the intron." We also need to special case insertions
                // between the edge of an exon and a donor or acceptor site and between a donor
                // or acceptor site and the intron
                IsWithinSpliceSiteRegion = variant.Overlaps(region.Start + 2, region.Start + 7) ||
                                           variant.Overlaps(region.End - 7, region.End - 2) ||
                                           variant.Overlaps(region.Start - 3, region.Start - 1) ||
                                           variant.Overlaps(region.End + 1, region.End + 3) ||
                                           isInsertion &&
                                           (variant.Start == region.Start ||
                                            variant.End == region.End ||
                                            variant.Start == region.Start + 2 ||
                                            variant.End == region.End - 2);
            }
        }

        public void DetermineExonicEffect(ITranscript transcript, IInterval variant, IMappedPosition position,
            int coveredCdnaStart, int coveredCdnaEnd, int coveredCdsStart, int coveredCdsEnd, string altAllele,
            bool startCodonInsertionWithNoImpact)
        {
            HasExonOverlap = position.ExonStart != -1 || position.ExonEnd != -1;

            if (transcript.Translation != null)
            {
                var codingRegion = transcript.Translation.CodingRegion;
                AfterCoding      = IsAfterCoding(variant.Start, variant.End, transcript.End, codingRegion.End);
                BeforeCoding     = IsBeforeCoding(variant.Start, variant.End, transcript.Start, codingRegion.Start);
                WithinCds        = IsWithinCds(coveredCdsStart, coveredCdsEnd, codingRegion, variant);
                IsCoding         = !startCodonInsertionWithNoImpact && (position.CdsStart != -1 || position.CdsEnd != -1);
            }

            WithinCdna = IsWithinCdna(coveredCdnaStart, coveredCdnaEnd, transcript.TotalExonLength);

            if (coveredCdsStart != -1 && coveredCdsEnd != -1)
            {
                var varLen    = coveredCdsEnd - coveredCdsStart + 1;
                var alleleLen = altAllele?.Length ?? 0;
                HasFrameShift = position.CdsStart != -1 && position.CdsEnd != -1 && !Codons.IsTriplet(alleleLen - varLen);
            }

            OverlapWithMicroRna = IsMatureMirnaVariant(position.CdnaStart, position.CdnaEnd, transcript.MicroRnas,
                transcript.BioType == BioType.miRNA);
        }

        private static bool HasExonRegionOverlap(ITranscriptRegion region, IInterval variant)
        {
            if (region == null || region.Type != TranscriptRegionType.Exon) return false;
            return region.Overlaps(variant);
        }

        internal static bool IsMatureMirnaVariant(int cdnaStart, int cdnaEnd, IInterval[] microRnas, bool isMiRna)
        {
            if (microRnas == null) return false;
            if (!isMiRna || cdnaStart == -1 || cdnaEnd == -1) return false;
            return microRnas.Any(microRna => microRna.Overlaps(cdnaStart, cdnaEnd));
        }

        internal static bool IsAfterCoding(int variantRefBegin, int variantRefEnd, int transcriptEnd, int codingRegionEnd)
        {
            // special case to handle insertions after the CDS end
            if (variantRefBegin == variantRefEnd + 1 && variantRefEnd == codingRegionEnd)
            {
                return true;
            }

            var result = IntervalUtilities.Overlaps(variantRefBegin, variantRefEnd, codingRegionEnd + 1, transcriptEnd);

            return result;
        }

        internal static bool IsBeforeCoding(int variantRefBegin, int variantRefEnd, int transcriptStart, int codingRegionStart)
        {
            // special case to handle insertions before the CDS start
            if (variantRefBegin == variantRefEnd + 1 && variantRefBegin == codingRegionStart) return true;

            bool result = IntervalUtilities.Overlaps(variantRefBegin, variantRefEnd, transcriptStart, codingRegionStart - 1);
            return result;
        }

        internal static bool IsWithinCdna(int coveredCdnaStart, int coveredCdnaEnd, int totalExonLen) =>
            coveredCdnaStart > 0 && coveredCdnaEnd <= totalExonLen;

        internal bool IsWithinCds(int coveredCdsBegin, int coveredCdsEnd, IInterval codingRegion, IInterval variant)
        {
            if (IsWithinFrameshiftIntron) return variant.Overlaps(codingRegion);
            return coveredCdsBegin != -1 && coveredCdsEnd != -1;
        }
    }
}