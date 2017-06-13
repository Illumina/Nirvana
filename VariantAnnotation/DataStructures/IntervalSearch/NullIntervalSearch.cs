using System.Collections.Generic;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.IntervalSearch
{
    /// <summary>
    /// rather than adding logic for dealing will null interval arrays, we will
    /// use this class to effectively act as a NOP
    /// </summary>
    public sealed class NullIntervalSearch<T> : IIntervalForest<T>, IIntervalSearch<T>
    {
        #region IIntervalForest

        public bool OverlapsAny(ushort referenceIndex, int begin, int end)
        {
            return false;
        }

        public void GetAllOverlappingValues(ushort referenceIndex, int begin, int end, List<T> values)
        {
            values.Clear();
        }

        #endregion

        #region IIntervalSearch

        public bool OverlapsAny(int begin, int end)
        {
            return false;
        }

        public void GetAllOverlappingValues(int begin, int end, List<T> values)
        {
            values.Clear();
        }

        public bool GetFirstOverlappingInterval(int begin, int end, out Interval<T> interval)
        {
            interval = IntervalArray<T>.EmptyInterval;
            return false;
        }

        #endregion
    }
}
