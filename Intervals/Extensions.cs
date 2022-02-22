namespace Intervals;

public static class Extensions
{
    public static bool Overlaps(this IInterval interval1, IInterval interval2, int flankingLength = 0) =>
        IntervalUtilities.Overlaps(interval1.Start - flankingLength, interval1.End + flankingLength,
            interval2.Start, interval2.End);

    public static bool Overlaps<T>(this T item, int start, int end) where T : IInterval =>
        IntervalUtilities.Overlaps(item.Start, item.End, start, end);

    public static bool Overlaps(this IInterval interval, int start, int end) => IntervalUtilities.Overlaps(
        interval.Start, interval.End, start, end);

    public static bool Contains(this IInterval interval1, IInterval interval2) => IntervalUtilities.Contains(
        interval1.Start, interval1.End, interval2.Start, interval2.End);
}