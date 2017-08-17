namespace VariantAnnotation.Interface.Intervals
{
	public interface IIntervalForest<out T>
	{
		bool OverlapsAny(ushort refIndex, int begin, int end);
		T[] GetAllOverlappingValues(ushort refIndex, int begin, int end);
	}
}