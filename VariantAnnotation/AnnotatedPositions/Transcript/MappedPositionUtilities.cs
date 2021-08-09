using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public static class MappedPositionUtilities
    {
        public static (int Index, ITranscriptRegion Region) FindRegion(ITranscriptRegion[] regions,
            int variantPosition)
        {
            int index  = regions.BinarySearch(variantPosition);
            var region = index < 0 ? null : regions[index];
            return (index, region);
        }

        public static (int CdnaStart, int CdnaEnd) GetCdnaPositions(ITranscriptRegion startRegion,
            ITranscriptRegion endRegion, int start, int end, bool onReverseStrand)
        {
            int cdnaStart = GetCdnaPosition(startRegion, start, onReverseStrand);
            int cdnaEnd   = GetCdnaPosition(endRegion,   end,   onReverseStrand);
            return (cdnaStart, cdnaEnd);
        }

        public static (int CdnaStart, int CdnaEnd) GetInsertionCdnaPositions(ITranscriptRegion startRegion,
            ITranscriptRegion endRegion, int start, int end, bool onReverseStrand)
        {
            int cdnaStart, cdnaEnd;

            if (onReverseStrand)
            {
                cdnaStart = GetCdnaPosition(startRegion, start, true);
                if (cdnaStart != -1) return (cdnaStart, cdnaStart + 1);

                cdnaEnd = GetCdnaPosition(endRegion, end, true);
                return cdnaEnd != -1 ? (cdnaEnd - 1, cdnaEnd) : (-1, -1);
            }

            cdnaEnd = GetCdnaPosition(endRegion, end, false);
            if (cdnaEnd != -1) return (cdnaEnd + 1, cdnaEnd);

            cdnaStart = GetCdnaPosition(startRegion, start, false);
            return cdnaStart != -1 ? (cdnaStart, cdnaStart - 1) : (-1, -1);
        }

        private static int GetCdnaPosition(ITranscriptRegion region, int variantPosition, bool onReverseStrand)
        {
            if (region == null || region.Type != TranscriptRegionType.Exon) return -1;

            return onReverseStrand
                ? region.End - variantPosition + region.CdnaStart
                : variantPosition - region.Start + region.CdnaStart;
        }

        /// <summary>
        /// Assuming at least one cDNA coordinate overlaps with an exon, the covered cDNA coordinates represent
        /// the coordinates actually covered by the variant.
        /// </summary>
        public static (int Start, int End) GetCoveredCdnaPositions(this ITranscriptRegion[] regions, int cdnaStart, int startRegionIndex,
            int cdnaEnd, int endRegionIndex, bool onReverseStrand)
        {
            // exon case
            if (cdnaStart != -1 && cdnaEnd != -1) return (cdnaStart, cdnaEnd);

            if (onReverseStrand) (startRegionIndex, endRegionIndex) = (endRegionIndex, startRegionIndex);

            var startRegion = regions.GetCoveredRegion(startRegionIndex);
            var endRegion   = regions.GetCoveredRegion(endRegionIndex);

            if (startRegion.Type != TranscriptRegionType.Exon && endRegion.Type != TranscriptRegionType.Exon)
                return (-1, -1);

            int codingEnd = onReverseStrand ? regions[0].CdnaEnd : regions[regions.Length - 1].CdnaEnd;

            cdnaStart = GetCoveredCdnaPosition(true, cdnaStart, startRegion, startRegionIndex, onReverseStrand, codingEnd);
            cdnaEnd   = GetCoveredCdnaPosition(false, cdnaEnd, endRegion, endRegionIndex, onReverseStrand, codingEnd);

            return cdnaStart < cdnaEnd ? (cdnaStart, cdnaEnd) : (cdnaEnd, cdnaStart);
        }

        private static ITranscriptRegion GetCoveredRegion(this ITranscriptRegion[] regions, int regionIndex)
        {
            if (regionIndex == -1) return regions[0];
            return regionIndex == ~regions.Length ? regions[regions.Length - 1] : regions[regionIndex];
        }

        private static int GetCoveredCdnaPosition(bool isStart, int cdnaPosition, ITranscriptRegion region, int regionIndex, bool onReverseStrand, int codingEnd)
        {
            if (cdnaPosition >= 0) return cdnaPosition;

            // start before transcript
            if (regionIndex == -1) return onReverseStrand ? codingEnd : 1;

            // end after transcript
            if (regionIndex < -1) return onReverseStrand ? 1 : codingEnd;

            // intron
            return isStart ? region.CdnaEnd : region.CdnaStart;
        }

        public static (int CdsStart, int CdsEnd, int ProteinStart, int ProteinEnd) GetCoveredCdsAndProteinPositions(
            int coveredCdnaStart, int coveredCdnaEnd, byte startExonPhase, ICodingRegion codingRegion)
        {
            if (codingRegion     == null                  ||
                coveredCdnaEnd   < codingRegion.CdnaStart ||
                coveredCdnaStart > codingRegion.CdnaEnd   ||
                coveredCdnaStart == -1 && coveredCdnaEnd == -1) return (-1, -1, -1, -1);

            if (coveredCdnaStart < codingRegion.CdnaStart) coveredCdnaStart = codingRegion.CdnaStart;
            if (coveredCdnaEnd   > codingRegion.CdnaEnd)   coveredCdnaEnd   = codingRegion.CdnaEnd;

            int offset = startExonPhase - codingRegion.CdnaStart + 1;
            int start  = coveredCdnaStart + offset;
            int end    = coveredCdnaEnd   + offset;

            return (start, end, GetProteinPosition(start), GetProteinPosition(end));
        }

        public static int GetProteinPosition(int cdsPosition)
        {
            if (cdsPosition == -1) return -1;
            return (cdsPosition + 2) / 3;
        }

        public static (int CdsStart, int CdsEnd) GetCdsPositions(ICodingRegion codingRegion, int cdnaStart,
            int cdnaEnd, byte startExonPhase, bool isInsertion)
        {
            int cdsStart = GetCdsPosition(codingRegion, cdnaStart, startExonPhase);
            int cdsEnd   = GetCdsPosition(codingRegion, cdnaEnd, startExonPhase);

            // silence CDS for insertions that occur just after the coding region
            if (isInsertion && codingRegion != null && (cdnaEnd == codingRegion.CdnaEnd || cdnaStart == codingRegion.CdnaStart))
            {
                cdsStart = -1;
                cdsEnd   = -1;
            }

            return (cdsStart, cdsEnd);
        }

        private static int GetCdsPosition(ICodingRegion codingRegion, int cdnaPosition, byte startExonPhase)
        {
            if (codingRegion == null || cdnaPosition < codingRegion.CdnaStart ||
                cdnaPosition > codingRegion.CdnaEnd) return -1;
            return cdnaPosition - codingRegion.CdnaStart + startExonPhase + 1;
        }

        // this is used to get CDS coordinates past the last CDS position
        public static int GetExtendedCdsPosition(int cdnaStart, int cdnaPosition, byte startExonPhase)
        {
            if (cdnaPosition < cdnaStart) return -1;
            return cdnaPosition - cdnaStart + startExonPhase + 1;
        }
    }
}