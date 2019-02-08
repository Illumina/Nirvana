using System;

namespace Intervals
{
    public static class Utilities
    {
        /// <summary>
        /// first interval overlaps with the second interval
        /// </summary>
        public static bool Overlaps(int firstStart, int firstEnd, int secondStart, int secondEnd) =>
            firstStart <= secondEnd && secondStart <= firstEnd;

        /// <summary>
        /// first interval contains the second interval
        /// </summary>
        public static bool Contains(int firstStart, int firstEnd, int secondStart, int secondEnd) =>
            firstStart <= secondStart && secondEnd <= firstEnd;

        /// <summary>
        /// get the intersection of the two intervals
        /// </summary>
        public static (int Start, int End) Intersects(int firstStart, int firstEnd, int secondStart, int secondEnd) =>
            Overlaps(firstStart, firstEnd, secondStart, secondEnd)
                ? (Math.Max(firstStart, secondStart), Math.Min(firstEnd, secondEnd))
                : (-1, -1);
    }
}