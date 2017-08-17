namespace VariantAnnotation.Sequence
{
	public struct Band
	{
		public readonly int Begin;
		public readonly int End;
		public readonly string Name;

		/// <summary>
		/// constructor
		/// </summary>
		public Band(int begin, int end, string name)
		{
			Begin = begin;
			End = end;
			Name = name;
		}

		/// <summary>
		/// our compare function
		/// </summary>
		public int Compare(int position)
		{
			if (position < Begin) return 1;
			return position > End ? -1 : 0;
		}
	}
}