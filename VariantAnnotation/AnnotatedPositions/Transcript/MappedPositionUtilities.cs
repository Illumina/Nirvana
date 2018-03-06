using VariantAnnotation.Algorithms;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public static class MappedPositionUtilities
    {
        public static (int Index, ITranscriptRegion Region) FindRegion(ITranscriptRegion[] regions,
            int variantPosition)
        {
            var index = regions.BinarySearch(variantPosition);
            var region = index < 0 ? null : regions[index];
            return (index, region);
        }

        public static (int CdnaStart, int CdnaEnd) GetCdnaPositions(ITranscriptRegion startRegion,
            ITranscriptRegion endRegion, IInterval variant, bool onReverseStrand, bool isInsertion)
        {
            var cdnaStart = GetCdnaPosition(startRegion, variant.Start, onReverseStrand);
            var cdnaEnd   = GetCdnaPosition(endRegion, variant.End, onReverseStrand);

            if (FoundExonEndpointInsertion(isInsertion, cdnaStart, cdnaEnd, startRegion, endRegion))
            {
                (cdnaStart, cdnaEnd) = FixExonEndpointInsertion(cdnaStart, cdnaEnd, onReverseStrand, startRegion,
                    endRegion, variant);
            }

            return (cdnaStart, cdnaEnd);
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

            if (onReverseStrand) Swap.Int(ref startRegionIndex, ref endRegionIndex);

            var startRegion = regions.GetCoveredRegion(startRegionIndex);
            var endRegion   = regions.GetCoveredRegion(endRegionIndex);

            if (startRegion.Type != TranscriptRegionType.Exon && endRegion.Type != TranscriptRegionType.Exon)
                return (-1, -1);

            var codingEnd = onReverseStrand ? regions[0].CdnaEnd : regions[regions.Length - 1].CdnaEnd;

            cdnaStart = GetCoveredCdnaPosition(cdnaStart, startRegion, startRegionIndex, onReverseStrand, codingEnd);
            cdnaEnd   = GetCoveredCdnaPosition(cdnaEnd, endRegion, endRegionIndex, onReverseStrand, codingEnd);

            return cdnaStart < cdnaEnd ? (cdnaStart, cdnaEnd) : (cdnaEnd, cdnaStart);
        }

        private static ITranscriptRegion GetCoveredRegion(this ITranscriptRegion[] regions, int regionIndex)
        {
            if (regionIndex == -1) return regions[0];
            if (regionIndex == ~regions.Length) return regions[regions.Length - 1];
            return regions[regionIndex];
        }

        private static int GetCoveredCdnaPosition(int cdnaPosition, ITranscriptRegion region, int regionIndex, bool onReverseStrand, int codingEnd)
        {
            if (cdnaPosition >= 0) return cdnaPosition;

            // start before transcript
            if (regionIndex == -1) return onReverseStrand ? codingEnd : 1;

            // end after transcript
            if (regionIndex < -1) return onReverseStrand ? 1 : codingEnd;

            // intron
            return onReverseStrand ? region.CdnaStart : region.CdnaEnd;
        }

        public static (int Start, int End) GetCoveredCdsPositions(int coveredCdnaStart, int coveredCdnaEnd,
            byte startExonPhase, ICodingRegion codingRegion)
        {
            if (codingRegion == null || 
                coveredCdnaEnd < codingRegion.CdnaStart || 
                coveredCdnaStart > codingRegion.CdnaEnd ||
                coveredCdnaStart == -1 && coveredCdnaEnd == -1) return (-1, -1);

            int beginOffset = startExonPhase - codingRegion.CdnaStart + 1;
            var start = coveredCdnaStart + beginOffset;
            var end   = coveredCdnaEnd + beginOffset;

            return (start, end);
        }

        public static int GetProteinPosition(int cdsPosition)
        {
            if (cdsPosition == -1) return -1;
            return (cdsPosition + 2) / 3;
        }

        public static (int CdsStart, int CdsEnd) GetCdsPositions(ICodingRegion codingRegion, int cdnaStart,
            int cdnaEnd, byte startExonPhase, bool isInsertion)
        {
            var cdsStart = GetCdsPosition(codingRegion, cdnaStart, startExonPhase);
            var cdsEnd   = GetCdsPosition(codingRegion, cdnaEnd, startExonPhase);

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

        /// <summary>
        /// Fixes the missing cDNA coordinate for situations where an insertion occurs on either the first or last
        /// base of an exon
        /// </summary>
        internal static (int CdnaStart, int CdnaEnd) FixExonEndpointInsertion(int cdnaStart, int cdnaEnd,
            bool onReverseStrand, ITranscriptRegion startRegion, ITranscriptRegion endRegion, IInterval variant)
        {
            var (intron, exon) = startRegion.Type == TranscriptRegionType.Exon
                ? (endRegion, startRegion)
                : (startRegion, endRegion);

            bool matchExonStart = variant.Start == exon.Start;

            int cdnaPos = !onReverseStrand && matchExonStart || onReverseStrand && !matchExonStart
                ? intron.CdnaStart
                : intron.CdnaEnd;

            if (cdnaStart == -1) cdnaStart = cdnaPos;
            else cdnaEnd = cdnaPos;

            return (cdnaStart, cdnaEnd);
        }

        /// <summary>
        /// Identifies when an insertion on an exon boundary needs special attention. Here we're looking for one
        /// intron & one exon where one cDNA coordinate is defined, but the other isn't.
        /// </summary>
        internal static bool FoundExonEndpointInsertion(bool isInsertion, int cdnaStart, int cdnaEnd,
            ITranscriptRegion startRegion, ITranscriptRegion endRegion)
        {
            bool isCdnaStartUndef = cdnaStart         == -1;
            bool isCdnaEndUndef   = cdnaEnd           == -1;
            bool isStartExon      = startRegion?.Type == TranscriptRegionType.Exon;
            bool isEndExon        = endRegion?.Type   == TranscriptRegionType.Exon;

            return isInsertion && startRegion != null && endRegion != null && isStartExon ^ isEndExon &&
                   isCdnaStartUndef ^ isCdnaEndUndef;
        }
    }
}