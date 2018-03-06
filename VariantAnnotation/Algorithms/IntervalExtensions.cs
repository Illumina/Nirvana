using VariantAnnotation.Interface.Intervals;

namespace VariantAnnotation.Algorithms
{
    public static class IntervalExtensions
    {
        /// <summary>
        /// interval 2 is overlapped with interval 1 +/- flanking length
        /// </summary>
        public static bool Overlaps(this IInterval interval1, IInterval interval2, int flankingLength = 0) =>
            IntervalUtilities.Overlaps(interval1.Start - flankingLength, interval1.End + flankingLength,
                interval2.Start, interval2.End);

        public static bool Overlaps(this IInterval interval, int start, int end) => IntervalUtilities.Overlaps(
            interval.Start, interval.End, start, end);

        public static bool Contains(this IInterval interval1, IInterval interval2) => IntervalUtilities.Contains(
            interval1.Start, interval1.End, interval2.Start, interval2.End);

        public static Interval Intersect(this IInterval interval1, IInterval interval2)
        {
            var (newStart, newEnd) = IntervalUtilities.Intersect(interval1.Start, interval1.End, interval2.Start, interval2.End);
            return new Interval(newStart, newEnd);
        }
    }
}