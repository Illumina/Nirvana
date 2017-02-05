using System.Collections.Generic;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.IntervalSearch
{
    public sealed class IntervalArray<T> : IIntervalSearch<T>
    {
        #region members

        private readonly Interval[] _array;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public IntervalArray(Interval[] array)
        {
            _array = array;
            SetMaxIntervals();
        }

        /// <summary>
        /// returns true if there are any overlapping intervals in the specified region
        /// </summary>
        public bool OverlapsAny(int begin, int end)
        {
            return GetFirstIndexAny(begin, end) >= 0;
        }

        /// <summary>
        /// returns values for all intervals that overlap the specified interval
        /// </summary>
        public void GetAllOverlappingValues(int begin, int end, List<T> values)
        {
            values.Clear();

            var firstIndex = GetFirstIndex(begin, end);
            if (firstIndex == -1) return;

            AddOverlappingValues(firstIndex, begin, end, values);
        }

        /// <summary>
        /// adds the overlapping values for all intervals overlapping the specified interval
        /// </summary>
        private void AddOverlappingValues(int firstIndex, int begin, int end, ICollection<T> values)
        {
            for (var index = firstIndex; index < _array.Length; index++)
            {
                var interval = _array[index];
                if (interval.Begin > end) break;
                if (interval.Overlaps(begin, end)) values.Add(interval.Value);
            }
        }

        /// <summary>
        /// finds the first index that overlaps on the interval [begin, max)
        /// </summary>
        private int GetFirstIndex(int intervalBegin, int intervalEnd)
        {
            var begin = 0;
            var end = _array.Length - 1;

            var lastOverlapIndex = -1;

            while (begin <= end)
            {
                var index = begin + (end - begin >> 1);

                if (_array[index].Overlaps(intervalBegin, intervalEnd)) lastOverlapIndex = index;
                var ret = _array[index].CompareMax(intervalBegin);

                if (ret <= 0) end = index - 1;
                else begin = index + 1;
            }

            return lastOverlapIndex;
        }

        /// <summary>
        /// finds the first index that overlaps on the interval [begin, max)
        /// </summary>
        private int GetFirstIndexAny(int intervalBegin, int intervalEnd)
        {
            var begin = 0;
            var end = _array.Length - 1;

            while (begin <= end)
            {
                var index = begin + (end - begin >> 1);

                if (_array[index].Overlaps(intervalBegin, intervalEnd)) return index;
                var ret = _array[index].CompareMax(intervalBegin);

                if (ret <= 0) end = index - 1;
                else begin = index + 1;
            }

            return ~begin;
        }

        /// <summary>
        /// sets the max endpoint for each interval element
        /// </summary>
        private void SetMaxIntervals()
        {
            var currentMax = int.MinValue;

            for (var i = 0; i < _array.Length; i++)
            {
                if (_array[i].End > currentMax) currentMax = _array[i].End;
                _array[i].Max = currentMax;
            }
        }

        public struct Interval
        {
            public readonly int Begin;
            public readonly int End;
            public readonly T Value;
            public int Max;

            /// <summary>
            /// constructor
            /// </summary>
            public Interval(int begin, int end, T value)
            {
                Begin = begin;
                End = end;
                Value = value;
                Max = -1;
            }

            /// <summary>
            /// our compare function
            /// </summary>
            public int CompareMax(int position)
            {
                if (position < Max) return -1;
                if (position > Max) return 1;
                return 0;
            }

            /// <summary>
            /// returns true if this interval overlaps with the specified interval
            /// </summary>
            public bool Overlaps(int intervalBegin, int intervalEnd)
            {
                return End >= intervalBegin && Begin <= intervalEnd;
            }

            /// <summary>
            /// returns a string representation of this interval
            /// </summary>
            public override string ToString()
            {
                return $"{Begin} - {End} ({Max}). Value: {Value}";
            }
        }
    }
}
