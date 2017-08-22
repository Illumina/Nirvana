using System.Linq;
using VariantAnnotation.Algorithms;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    // TODO: this class should be able to be simplified by coupling with mappedPosition
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

        public void DetermineIntronicEffect(IInterval[] introns, IInterval variant, VariantType variantType)
        {
            if (introns == null) return;

            var isInsertion = variantType == VariantType.insertion;

            foreach (var intron in introns)
            {
                // TODO: we should sort our introns so that we can end early

                // skip this one if variant is out of range : the range is set to 3 instead of the original old:
                // all of the checking occured in the region between start-3 to end+3, if we set to 8, we can made mistakes when
                //checking IsWithinIntron when we have a small exon
                if (!variant.Overlaps(intron.Start - 3, intron.End + 3)) continue;

                // under various circumstances the genebuild process can introduce artificial 
                // short (<= 12 nucleotide) introns into transcripts (e.g. to deal with errors
                // in the reference sequence etc.), we don't want to categorize variations that
                // fall in these introns as intronic, or as any kind of splice variant

                var isFrameshiftIntron = intron.End - intron.Start <= 12;

                if (isFrameshiftIntron)
                {
                    if (variant.Overlaps(intron.Start, intron.End))
                    {
                        IsWithinFrameshiftIntron = true;
                        continue;
                    }
                }

                if (variant.Overlaps(intron.Start, intron.Start + 1))
                {
                    IsStartSpliceSite = true;
                }

                if (variant.Overlaps(intron.End - 1, intron.End))
                {
                    IsEndSpliceSite = true;
                }

                // we need to special case insertions between the donor and acceptor sites

                //make sure the size of intron is larger than 4
                if (intron.Start <= intron.End - 4)
                {
                    if (variant.Overlaps(intron.Start + 2, intron.End - 2) ||
                        isInsertion && (variant.Start == intron.Start + 2
                                        || variant.End == intron.End - 2))
                    {
                        IsWithinIntron = true;
                    }
                }

                // the definition of splice_region (SO:0001630) is "within 1-3 bases of the
                // exon or 3-8 bases of the intron." We also need to special case insertions
                // between the edge of an exon and a donor or acceptor site and between a donor
                // or acceptor site and the intron
                IsWithinSpliceSiteRegion = variant.Overlaps(intron.Start + 2, intron.Start + 7) ||
                                           variant.Overlaps(intron.End - 7, intron.End - 2) ||
                                           variant.Overlaps(intron.Start - 3, intron.Start - 1) ||
                                           variant.Overlaps(intron.End + 1, intron.End + 3) ||
                                           isInsertion &&
                                           (variant.Start == intron.Start ||
                                            variant.End == intron.End ||
                                            variant.Start == intron.Start + 2 ||
                                            variant.End == intron.End - 2);
            }
        }

        public void DetermineExonicEffect(ITranscript transcript, IInterval variant, IMappedPositions mappedPositions,
            string altAllele, bool insertionNoImpact)
        {
            HasExonOverlap = ExonOverlaps(transcript.CdnaMaps, variant);

            if (transcript.Translation != null)
            {
                AfterCoding  = IsAfterCoding(variant.Start, variant.End, transcript.End, transcript.Translation.CodingRegion.End);
                BeforeCoding = IsBeforeCoding(variant.Start, variant.End, transcript.Start, transcript.Translation.CodingRegion.Start);
                WithinCds    = IsWithinCds(transcript.CdnaMaps, transcript.Translation, variant.Start, variant.End);
                IsCoding     = !insertionNoImpact && (mappedPositions.CdsInterval.Start != null || mappedPositions.CdsInterval.End != null);
            }

            WithinCdna = IsWithinCdna(mappedPositions.ImpactedCdnaInterval, transcript.TotalExonLength);

            if (mappedPositions.ImpactedCdsInterval != null)
            {
                var varLen    = mappedPositions.ImpactedCdsInterval.End - mappedPositions.ImpactedCdsInterval.Start + 1;
                var alleleLen = altAllele?.Length ?? 0;
                HasFrameShift = mappedPositions.CdsInterval.Start != null && mappedPositions.CdsInterval.End != null && !Codons.IsTriplet(alleleLen - varLen);
            }

            OverlapWithMicroRna = IsMatureMirnaVariant(mappedPositions, transcript.MicroRnas,
                transcript.BioType == BioType.miRNA);
        }

        internal static bool ExonOverlaps(ICdnaCoordinateMap[] cdnaMaps, IInterval variant)
        {
            foreach (var cdnaMap in cdnaMaps) if (cdnaMap.Overlaps(variant)) return true;
            return false;
        }

        /// <summary>
        /// returns true if the variant overlaps a mature MiRNA. [VariationEffect.pm:432 within_mature_miRNA]
        /// </summary>
        internal static bool IsMatureMirnaVariant(IMappedPositions mappedPositions, IInterval[] microRnas, bool isMiRna)
        {
            if (microRnas == null) return false;

            if (!isMiRna || !mappedPositions.CdnaInterval.Start.HasValue ||
                !mappedPositions.CdnaInterval.End.HasValue) return false;

            return microRnas.Any(microRna => microRna.Overlaps(mappedPositions.CdnaInterval.Start.Value, mappedPositions.CdnaInterval.End.Value));
        }

        /// <summary>
        /// returns true if the variant occurs before the coding start position [VariationEffect.pm:513 _after_coding]
        /// </summary>
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

        /// <summary>
        /// returns true if the variant occurs before the coding start position [VariationEffect.pm:561 _before_coding]
        /// </summary>
        internal static bool IsBeforeCoding(int variantRefBegin, int variantRefEnd, int transcriptStart, int codingRegionStart)
        {
            // special case to handle insertions before the CDS start
            if (variantRefBegin == variantRefEnd + 1 && variantRefBegin == codingRegionStart)
            {
                return true;
            }

            bool result = IntervalUtilities.Overlaps(variantRefBegin, variantRefEnd, transcriptStart, codingRegionStart - 1);

            return result;
        }

        /// <summary>
        /// returns true if the variant occurs within the cDNA [VariationEffect.pm:469 within_cdna]
        /// </summary>
        internal static bool IsWithinCdna(IInterval impactedCdnaInterval, int totalExonLen) =>
            impactedCdnaInterval.Start > 0 && impactedCdnaInterval.End <= totalExonLen;

        // TODO: update this function use mappedPosition
        ///<summary>
        /// returns true if the variant occurs within the CDS [VariationEffect.pm:501 within_cds]
        /// </summary>
        internal bool IsWithinCds(ICdnaCoordinateMap[] cdnaMaps, ITranslation translation, int start, int end)
        {
            if (translation == null) return false;

            // Here I have a complete refactoring of this code.
            // the name seems to suggests that any overlap between the variant and transcript coding region should set it to true
            bool result = false;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var cdnaCoordinateMap in cdnaMaps)
            {
                var cdsStartGenomic = cdnaCoordinateMap.Start > translation.CodingRegion.Start ?
                    cdnaCoordinateMap.Start : translation.CodingRegion.Start;

                // this is the genomic start position of the cds of this cdnaCoordinate if it exists
                // it does not exist if this exon is completely a UTR. In that case, it is set to the transcript
                // coding region start
                var cdsEndGenomic = cdnaCoordinateMap.End < translation.CodingRegion.End ?
                    cdnaCoordinateMap.End : translation.CodingRegion.End;

                if (IntervalUtilities.Overlaps(start, end, cdsStartGenomic, cdsEndGenomic)) return true;
            }

            // we also need to check if the vf is in a frameshift intron within the CDS
            if (IsWithinFrameshiftIntron)
            {
                result = translation.CodingRegion.Overlaps(start, end);
            }

            return result;
        }
    }
}