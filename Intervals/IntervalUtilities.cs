using System;

namespace Intervals;

public static class IntervalUtilities
{
    public static bool Overlaps(int firstStart, int firstEnd, int secondStart, int secondEnd) =>
        firstStart <= secondEnd && secondStart <= firstEnd;

    public static bool Contains(int firstStart, int firstEnd, int secondStart, int secondEnd) =>
        firstStart <= secondStart && secondEnd <= firstEnd;

    public static (int Start, int End) Intersects(int firstStart, int firstEnd, int secondStart, int secondEnd) =>
        Overlaps(firstStart, firstEnd, secondStart, secondEnd)
            ? (Math.Max(firstStart, secondStart), Math.Min(firstEnd, secondEnd))
            : (-1, -1);

    public static IntervalArray<T>.Interval[] CreateIntervals<T>(T[] sortedItems) where T : IInterval
    {
        if (sortedItems == null) return null;
        
        var currentMax = int.MinValue;
        var array      = new IntervalArray<T>.Interval[sortedItems.Length];

        for (var i = 0; i < sortedItems.Length; i++)
        {
            T item = sortedItems[i];

            if (item.End > currentMax) currentMax = item.End;
            array[i] = new IntervalArray<T>.Interval(item.Start, item.End, item, currentMax);
        }

        return array;
    }
}