using System;

namespace Intervals
{
    public static class Utilities
    {
        public static bool Overlaps(int firstStart, int firstEnd, int secondStart, int secondEnd) =>
            firstStart <= secondEnd && secondStart <= firstEnd;

        public static bool Contains(int firstStart, int firstEnd, int secondStart, int secondEnd) =>
            firstStart <= secondStart && secondEnd <= firstEnd;

        // given two intervals T and V, describe how V overlaps T
        public static OverlapType GetOverlapType(int tStart, int tEnd, int vStart, int vEnd)
        {
            if (tEnd < vStart || vEnd < tStart) return OverlapType.None;

            if (vStart >= tStart && vEnd <= tEnd) return OverlapType.CompletelyWithin;

            if (tStart >= vStart && tEnd <= vEnd) return OverlapType.CompletelyOverlaps;
            return OverlapType.Partial;
        }

        public static EndpointOverlapType GetEndpointOverlapType(int tStart, int tEnd, int vStart, int vEnd)
        {
            bool overlapsStart = Overlaps(tStart, tStart, vStart, vEnd);
            bool overlapsEnd   = Overlaps(tEnd,   tEnd,   vStart, vEnd);

            if (!overlapsStart && !overlapsEnd) return EndpointOverlapType.None;
            if (overlapsStart  && overlapsEnd) return EndpointOverlapType.Both;
            return overlapsStart ? EndpointOverlapType.Start : EndpointOverlapType.End;
        }

        public static (int Start, int End) Intersects(int firstStart, int firstEnd, int secondStart, int secondEnd) =>
            Overlaps(firstStart, firstEnd, secondStart, secondEnd)
                ? (Math.Max(firstStart, secondStart), Math.Min(firstEnd, secondEnd))
                : (-1, -1);
    }
}