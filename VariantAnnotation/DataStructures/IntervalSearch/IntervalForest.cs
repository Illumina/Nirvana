using System;
using System.Collections.Generic;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.IntervalSearch
{
    public sealed class IntervalForest<T> : IIntervalForest<T>
    {
        private readonly IntervalArray<T>[] _intervalArrays;
        private readonly ushort _maxIndex;

        /// <summary>
        /// constructor
        /// </summary>
        public IntervalForest(IntervalArray<T>[] intervalArrays)
        {
            _intervalArrays = intervalArrays;
            _maxIndex = (ushort)(intervalArrays.Length - 1);
        }

        /// <summary>
        /// returns values for all intervals that overlap the specified interval
        /// </summary>
        public bool OverlapsAny(ushort refIndex, int begin, int end)
        {
            if (refIndex > _maxIndex) throw new ArgumentOutOfRangeException($"The specified reference index ({refIndex}) is larger than the max index ({_maxIndex}).");
            var intervalArray = _intervalArrays[refIndex];
            return intervalArray.OverlapsAny(begin, end);
        }

        /// <summary>
        /// returns values for all intervals that overlap the specified interval
        /// </summary>
        public void GetAllOverlappingValues(ushort refIndex, int begin, int end, List<T> values)
        {
	        if (refIndex > _maxIndex)
	        {
		        //throw new ArgumentOutOfRangeException($"The specified reference index ({refIndex}) is larger than the max index ({_maxIndex}).");
				//we should not throw an exception, just return empty list, since this can happen for any unknown refSeq and Nirvana should not crash because of that
				values.Clear();
		        return;
	        }
            var intervalArray = _intervalArrays[refIndex];
            intervalArray.GetAllOverlappingValues(begin, end, values);
        }
    }
}
