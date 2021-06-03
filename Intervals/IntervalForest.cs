namespace Intervals
{
	public sealed class IntervalForest<T> : IIntervalForest<T>
	{
		private readonly IntervalArray<T>[] _intervalArrays;
		private readonly ushort _maxIndex;

		public IntervalForest(IntervalArray<T>[] intervalArrays)
		{
			_intervalArrays = intervalArrays;
			_maxIndex       = (ushort)(intervalArrays.Length - 1);
		}

		/// <summary>
		/// returns whether there is any interval that overlaps the specified interval
		/// </summary>
		public bool OverlapsAny(ushort refIndex, int begin, int end)
		{
			if (refIndex > _maxIndex) return false;
			var intervalArray = _intervalArrays[refIndex];
			if (intervalArray == null) return false;
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
		
		public Interval<T>[] GetAllOverlappingIntervals(ushort refIndex, int begin, int end)
		{
			if (refIndex > _maxIndex) return null;
			var intervalArray = _intervalArrays[refIndex];
			return intervalArray?.GetAllOverlappingIntervals(begin, end);
		}
	}
}