using Cache.Data;

namespace VariantAnnotation.Caches.DataStructures
{
    public static class TranscriptRegionExtensions
    {
        public static int BinarySearch(this TranscriptRegion[] regions, int position)
        {
            var begin = 0;
            var end   = regions.Length - 1;

            while (begin <= end)
            {
                var index  = begin + (end - begin >> 1);
                var region = regions[index];

                if (position >= region.Start && position <= region.End) return index;
                if (region.End < position) begin = index + 1;
                else if (position < region.Start) end = index - 1;
            }

            return ~begin;
        }

        public static (int ExonStart, int ExonEnd, int IntronStart, int IntronEnd) GetExonsAndIntrons(
            this TranscriptRegion[] regions, int startIndex, int endIndex)
        {
            int affectedStartIndex = GetAffectedRegionIndex(startIndex);
            int affectedEndIndex   = GetAffectedRegionIndex(endIndex);

            var exons   = regions.FindDesiredRegionIds(TranscriptRegionType.Exon, affectedStartIndex, affectedEndIndex);
            var introns = regions.FindDesiredRegionIds(TranscriptRegionType.Intron, affectedStartIndex, affectedEndIndex);

            return (exons.Start, exons.End, introns.Start, introns.End);
        }

        private static (int Start, int End) FindDesiredRegionIds(this TranscriptRegion[] regions,
            TranscriptRegionType desiredType, int startIndex, int endIndex)
        {
            var regionStart   = FindFirst(regions, desiredType, startIndex, endIndex);
            var newStartIndex = regionStart != -1 ? regionStart : startIndex;
            var regionEnd     = FindLast(regions, desiredType, newStartIndex, endIndex);

            var startId = regionStart == -1 ? -1 : regions[regionStart].Id;
            var endId   = regionEnd   == -1 ? -1 : regions[regionEnd].Id;

            if (endId < startId) (startId, endId) = (endId, startId);
            return (startId, endId);
        }

        private static int FindFirst(TranscriptRegion[] regions, TranscriptRegionType desiredType, int startIndex,
            int endIndex)
        {
            for (int i = startIndex; i <= endIndex; i++) if (regions[i].Type == desiredType) return i;
            return -1;
        }

        private static int FindLast(TranscriptRegion[] regions, TranscriptRegionType desiredType, int startIndex,
            int endIndex)
        {
            for (int i = endIndex; i >= startIndex; i--) if (regions[i].Type == desiredType) return i;
            return -1;
        }

        private static int GetAffectedRegionIndex(int index)
        {
            if (index >= 0) return index;
            index = ~index;
            return index == 0 ? 0 : index - 1;
        }
    }
}
