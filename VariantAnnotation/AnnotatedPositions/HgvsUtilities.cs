using System;
using Cache.Data;
using Genome;
using Intervals;
using OptimizedCore;
using Variants;

namespace VariantAnnotation.AnnotatedPositions
{
    public static class HgvsUtilities
    {
        public static PositionOffset GetPositionOffset(Transcript transcript, int position, int regionIndex)
        {
            if (!transcript.Overlaps(position, position)) return null;

            bool   onReverseStrand   = transcript.Gene.OnReverseStrand;
            var    region            = transcript.TranscriptRegions[regionIndex];
            int    codingRegionStart = transcript.CodingRegion?.CdnaStart ?? -1;
            int    codingRegionEnd   = transcript.CodingRegion?.CdnaEnd   ?? -1;
            ushort cdsOffset         = transcript.CodingRegion?.CdsOffset ?? 0;

            (int CdnaPosition, int Offset) po = region.Type == TranscriptRegionType.Exon
                ? (MappedPositionUtilities.GetCdnaPosition(region, position, onReverseStrand), 0)
                : GetIntronPositionAndOffset(position, region, onReverseStrand);

            if (po.CdnaPosition == -1) return null;

            (string cdsString, bool hasStopCodonNotation) =
                GetCdsString(po.CdnaPosition, codingRegionStart, codingRegionEnd, cdsOffset);
            string offset = po.Offset == 0 ? "" : po.Offset.ToString("+0;-0;+0");
            var    value  = $"{cdsString}{offset}";

            return new PositionOffset(po.CdnaPosition, po.Offset, value, hasStopCodonNotation);
        }

        private static (int Position, int Offset) GetIntronPositionAndOffset(int position, TranscriptRegion region,
            bool onReverseStrand)
        {
            int leftDist  = position   - region.Start + 1;
            int rightDist = region.End - position     + 1;

            int offset = Math.Min(leftDist, rightDist);
            if (!onReverseStrand && rightDist < leftDist || onReverseStrand && rightDist > leftDist) offset = -offset;

            // cDNA position truth table
            //
            //          forward     reverse
            //       -------------------------
            // L < R | CdnaStart | CdnaEnd   |
            // L = R | CdnaStart | CdnaStart |
            // L > R | CdnaEnd   | CdnaStart |
            //       -------------------------

            int cdnaPosition = leftDist < rightDist && onReverseStrand || leftDist > rightDist && !onReverseStrand
                ? region.CdnaEnd
                : region.CdnaStart;

            return (cdnaPosition, offset);
        }

        private static (string Cds, bool HasStopCodonNotation) GetCdsString(int position, int codingRegionStart,
            int codingRegionEnd, ushort cdsOffset)
        {
            string cdsString            = null;
            var    hasStopCodonNotation = false;

            if (codingRegionEnd != -1)
            {
                if (position > codingRegionEnd)
                {
                    cdsString            = "*" + (position - codingRegionEnd);
                    hasStopCodonNotation = true;
                }
            }

            if (!hasStopCodonNotation && codingRegionStart != -1)
            {
                cdsString = (position + (position >= codingRegionStart ? 1 : 0) - codingRegionStart + cdsOffset).ToString();
            }

            cdsString ??= position.ToString();
            return (cdsString, hasStopCodonNotation);
        }

        public static string AdjustTranscriptRefAllele(string transcriptRefAllele, int coveredCdnaStart,
            int coveredCdnaEnd, string cdnaSequence)
        {
            if (coveredCdnaStart == -1 || coveredCdnaEnd == -1 || cdnaSequence == null) return transcriptRefAllele;
            
            return coveredCdnaEnd < coveredCdnaStart
                ? string.Empty
                : cdnaSequence.Substring(coveredCdnaStart - 1, coveredCdnaEnd - coveredCdnaStart + 1);
        }

        public static string GetTranscriptAllele(string variantAllele, bool onReverseStrand) =>
            onReverseStrand ? SequenceUtilities.GetReverseComplement(variantAllele) : variantAllele;

        public static string FormatDnaNotation(string start, string end, string referenceId, string referenceBases,
            string alternateBases, GenomicChange type, char notationType)
        {
            var sb = StringBuilderCache.Acquire();

            // all start with transcript name & numbering type
            sb.Append(referenceId + ':' + notationType + '.');

            // handle single and multiple positions
            string coordinates = start == end
                ? start
                : start + '_' + end;

            // format rest of string according to type
            // note: inversion and multiple are never assigned as genomic changes
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (type)
            {
                case GenomicChange.Deletion:
                    sb.Append(coordinates + "del");
                    break;
                case GenomicChange.Inversion:
                    sb.Append(coordinates + "inv");
                    break;
                case GenomicChange.Duplication:
                    sb.Append(coordinates + "dup");
                    break;
                case GenomicChange.Substitution:
                    if (referenceBases == alternateBases)
                    {
                        sb.Append(start + '=');
                    }
                    else
                    {
                        sb.Append(start + referenceBases + '>' + alternateBases);
                    }

                    break;
                case GenomicChange.DelIns:
                    // NOTE: change to delins, now use del--ins-- to reduce Anavrin differences
                    sb.Append(coordinates + "delins" + alternateBases);
                    break;
                case GenomicChange.Insertion:
                    sb.Append(coordinates + "ins" + alternateBases);
                    break;

                default:
                    throw new InvalidOperationException("Unhandled genomic change found: " + type);
            }

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        public static bool IsDuplicateWithinInterval(ISequence refSequence, ISimpleVariant variant, IInterval interval,
            bool onReverseStrand)
        {
            if (variant.Type != VariantType.insertion) return false;

            int    altAlleleLen = variant.AltAllele.Length;
            string compareRegion;

            if (onReverseStrand)
            {
                if (variant.End + altAlleleLen > interval.End) return false;
                compareRegion = refSequence.Substring(variant.Start - 1, altAlleleLen);
            }
            else
            {
                if (variant.Start - altAlleleLen < interval.Start) return false;
                compareRegion = refSequence.Substring(variant.End - altAlleleLen, altAlleleLen);
            }

            return compareRegion == variant.AltAllele;
        }
    }
}