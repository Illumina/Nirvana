using System.Collections.Generic;

namespace Intervals
{
	public sealed class IntervalArray<T> : IIntervalSearch<T>
    {
		public readonly Interval<T>[] Array;

        public IntervalArray(Interval<T>[] array)
		{
			Array = array;
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
		public T[] GetAllOverlappingValues(int begin, int end)
		{
			var firstIndex = GetFirstIndex(begin, end);
			return firstIndex == -1 ? null : AddOverlappingValues(firstIndex, begin, end);
		}

        public Interval<T>[] GetAllOverlappingIntervals(int begin, int end)
        {
            var intervals = new List<Interval<T>>();
            var firstIndex = GetFirstIndex(begin, end);
            if (firstIndex == -1) return null;
            for (var index = firstIndex; index < Array.Length; index++)
            {
                var interval = Array[index];
                if (interval.Begin > end) break;
                if (interval.Overlaps(begin, end)) intervals.Add(interval);
            }

            return intervals.ToArray();
        }

        /// <summary>
		/// adds the overlapping values for all intervals overlapping the specified interval
		/// </summary>
		private T[] AddOverlappingValues(int firstIndex, int begin, int end)
		{
			var values = new List<T>();
			for (var index = firstIndex; index < Array.Length; index++)
			{
				var interval = Array[index];
				if (interval.Begin > end) break;
				if (interval.Overlaps(begin, end)) values.Add(interval.Value);
			}
			return values.ToArray();
		}

		/// <summary>
		/// finds the first index that overlaps on the interval [begin, max)
		/// </summary>
		private int GetFirstIndex(int intervalBegin, int intervalEnd)
		{
			var begin = 0;
			var end = Array.Length - 1;

			var lastOverlapIndex = -1;

			while (begin <= end)
			{
				var index = begin + (end - begin >> 1);

				if (Array[index].Overlaps(intervalBegin, intervalEnd)) lastOverlapIndex = index;
				var ret = Array[index].CompareMax(intervalBegin);

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
			var end = Array.Length - 1;

			while (begin <= end)
			{
				var index = begin + (end - begin >> 1);

				if (Array[index].Overlaps(intervalBegin, intervalEnd)) return index;
				var ret = Array[index].CompareMax(intervalBegin);

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

			for (var i = 0; i < Array.Length; i++)
			{
				if (Array[i].End > currentMax) currentMax = Array[i].End;
				Array[i].Max = currentMax;
			}
		}
    }
}