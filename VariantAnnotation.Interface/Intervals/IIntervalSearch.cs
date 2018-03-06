namespace VariantAnnotation.Interface.Intervals
{
	public interface IIntervalSearch<T>
	{
	    bool GetFirstOverlappingInterval(int begin, int end, out Interval<T> interval);
		T[] GetAllOverlappingValues(int begin, int end);
	}

	public struct Interval<T>
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
			return position > Max ? 1 : 0;
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

		public bool Contains(int position) => position >= Begin && position <= End;
	}
}