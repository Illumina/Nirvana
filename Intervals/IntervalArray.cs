using System;
using System.Collections.Generic;

namespace Intervals;

public sealed class IntervalArray<T>
{
    private readonly Interval[] _sortedArray;
    private readonly bool       _hasItems;
    private readonly int        _size;

    public IntervalArray(Interval[] sortedArray)
    {
        _sortedArray = sortedArray;
        _hasItems    = sortedArray != null;
        _size        = sortedArray?.Length ?? 0;
    }

    public void AddOverlappingValues(List<T> overlappingIntervals, int begin, int end)
    {
        if (!_hasItems) return;
        Span<Interval> span = GetFirstIndex(begin, end);
        if (span.Length == 0) return;

        foreach (Interval item in span)
        {
            if (item.Start > end) break;
            if (item.End >= begin && item.Start <= end) overlappingIntervals.Add(item.Value);
        }
    }

    private Span<Interval> GetFirstIndex(int intervalBegin, int intervalEnd)
    {
        var begin = 0;
        int end   = _size - 1;

        int lastOverlapIndex = -1;

        while (begin <= end)
        {
            int      index = begin + (end - begin >> 1);
            Interval item  = _sortedArray[index];

            if (item.End >= intervalBegin && item.Start <= intervalEnd) lastOverlapIndex = index;
            int ret = intervalBegin.CompareTo(item.Max);

            if (ret <= 0) end = index - 1;
            else begin        = index + 1;
        }

        return lastOverlapIndex == -1
            ? Span<Interval>.Empty
            : _sortedArray.AsSpan().Slice(lastOverlapIndex);
    }

    public readonly struct Interval
    {
        public readonly int Start;
        public readonly int End;
        public readonly T   Value;
        public readonly int Max;

        public Interval(int start, int end, T value, int max)
        {
            Start = start;
            End   = end;
            Value = value;
            Max   = max;
        }
        
        public bool Contains(int position) => position >= Start && position <= End;
    }
}