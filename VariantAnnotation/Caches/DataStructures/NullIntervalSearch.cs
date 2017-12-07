using VariantAnnotation.Interface.Intervals;

namespace VariantAnnotation.Caches.DataStructures
{
	public sealed class NullIntervalSearch<T> : IIntervalForest<T>, IIntervalSearch<T>
	{
		#region IIntervalForest

		public bool OverlapsAny(ushort referenceIndex, int begin, int end)
		{
			return false;
		}

		public T[] GetAllOverlappingValues(ushort referenceIndex, int begin, int end)
		{
			return null;
		}

		#endregion

		#region IIntervalSearch

		public T[] GetAllOverlappingValues(int begin, int end)
		{
			return null;
		}

		public bool GetFirstOverlappingInterval(int begin, int end, out Interval<T> interval)
		{
			interval = IntervalArray<T>.EmptyInterval;
			return false;
		}

		#endregion
	}
}