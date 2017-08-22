namespace VariantAnnotation.Algorithms
{
    public static class IntervalUtilities
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
    }
}