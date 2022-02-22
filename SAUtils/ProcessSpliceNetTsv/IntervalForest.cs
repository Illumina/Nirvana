using System;
using Intervals;

namespace SAUtils.ProcessSpliceNetTsv
{
    public sealed class IntervalForest<T>
    {
        private readonly OldIntervalArray<T>[] _intervalArrays;
        private readonly ushort                _maxIndex;

        public IntervalForest(OldIntervalArray<T>[] intervalArrays)
        {
            _intervalArrays = intervalArrays;
            _maxIndex       = (ushort) (intervalArrays.Length - 1);
        }

        /// <summary>
        /// returns whether there is any interval that overlaps the specified interval
        /// </summary>
        public bool OverlapsAny(ushort refIndex, int begin, int end)
        {
            if (refIndex > _maxIndex)
                throw new ArgumentOutOfRangeException(
                    $"The specified reference index ({refIndex}) is larger than the max index ({_maxIndex}).");
            var intervalArray = _intervalArrays[refIndex];
            return intervalArray.OverlapsAny(begin, end);
        }

        /// <summary>
        /// returns values for all intervals that overlap the specified interval
        /// </summary>
        public T[] GetAllOverlappingValues(ushort refIndex, int begin, int end)
        {
            if (refIndex > _maxIndex) return null;
            var intervalArray = _intervalArrays[refIndex];
            return intervalArray?.GetAllOverlappingValues(begin, end);
        }
    }
}