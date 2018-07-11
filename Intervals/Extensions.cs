namespace Intervals
{
    public static class Extensions
    {
        /// <summary>
        /// interval 2 is overlapped with interval 1 +/- flanking length
        /// </summary>
        public static bool Overlaps(this IInterval interval1, IInterval interval2, int flankingLength = 0) =>
            Utilities.Overlaps(interval1.Start - flankingLength, interval1.End + flankingLength,
                interval2.Start, interval2.End);

        public static bool Overlaps(this IInterval interval, int start, int end) => Utilities.Overlaps(
            interval.Start, interval.End, start, end);

        public static bool Contains(this IInterval interval1, IInterval interval2) => Utilities.Contains(
            interval1.Start, interval1.End, interval2.Start, interval2.End);

        public static Interval Intersects(this IInterval interval1, IInterval interval2)
        {
            (int start, int end) = Utilities.Intersects(interval1.Start, interval1.End, interval2.Start, interval2.End);
            return new Interval(start, end);
        }
    }
}