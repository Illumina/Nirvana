using System;
using VariantAnnotation.Algorithms;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.Caches.DataStructures
{
    public static class TranscriptRegionExtensions
    {
        public static int BinarySearch(this ITranscriptRegion[] regions, int position)
        {
            var begin = 0;
            int end   = regions.Length - 1;

            while (begin <= end)
            {
                int index  = begin + (end - begin >> 1);
                var region = regions[index];

                if (position >= region.Start && position <= region.End) return index;
                if (region.End < position) begin = index + 1;
                else if (position < region.Start) end = index - 1;
            }

            return ~begin;
        }

        public static (int ExonStart, int ExonEnd, int IntronStart, int IntronEnd) GetExonsAndIntrons(
            this ITranscriptRegion[] regions, int startIndex, int endIndex)
        {
            int affectedStartIndex = GetAffectedRegionIndex(startIndex);
            int affectedEndIndex   = GetAffectedRegionIndex(endIndex);

            if (affectedEndIndex < affectedStartIndex) Swap.Int(ref affectedStartIndex, ref affectedEndIndex);

            var exons   = regions.FindDesiredRegionIds(x => x == TranscriptRegionType.Exon || x == TranscriptRegionType.Gap, affectedStartIndex, affectedEndIndex);
            var introns = regions.FindDesiredRegionIds(x => x == TranscriptRegionType.Intron, affectedStartIndex, affectedEndIndex);

            return (exons.Start, exons.End, introns.Start, introns.End);
        }

        private static (int Start, int End) FindDesiredRegionIds(this ITranscriptRegion[] regions,
            Func<TranscriptRegionType, bool> hasDesiredRegion, int startIndex, int endIndex)
        {
            int regionStart   = FindFirst(regions, hasDesiredRegion, startIndex, endIndex);
            int newStartIndex = regionStart != -1 ? regionStart : startIndex;
            int regionEnd     = FindLast(regions, hasDesiredRegion, newStartIndex, endIndex);

            int startId = regionStart == -1 ? -1 : regions[regionStart].Id;
            int endId   = regionEnd   == -1 ? -1 : regions[regionEnd].Id;

            if (endId < startId) Swap.Int(ref startId, ref endId);
            return (startId, endId);
        }

        private static int FindFirst(ITranscriptRegion[] regions, Func<TranscriptRegionType, bool> hasDesiredRegion, int startIndex,
            int endIndex)
        {
            for (int i = startIndex; i <= endIndex; i++) if (hasDesiredRegion(regions[i].Type)) return i;
            return -1;
        }

        private static int FindLast(ITranscriptRegion[] regions, Func<TranscriptRegionType, bool> hasDesiredRegion, int startIndex,
            int endIndex)
        {
            for (int i = endIndex; i >= startIndex; i--) if (hasDesiredRegion(regions[i].Type)) return i;
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
