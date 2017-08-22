namespace VariantAnnotation.Interface.Intervals
{
	public struct NullableInterval
	{
		public readonly int? Start;
		public readonly int? End;

	    public NullableInterval(int? start, int? end)
	    {
	        Start = start;
	        End = end;
	    }
	}
}