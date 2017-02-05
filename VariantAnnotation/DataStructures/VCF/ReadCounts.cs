namespace VariantAnnotation.DataStructures.VCF
{
	internal sealed class ReadCounts
	{
		#region members

		private readonly IIntermediateSampleFields _tmp;

		#endregion

		// constructor
		public ReadCounts(IIntermediateSampleFields intermediateSampleFields)
		{
			_tmp = intermediateSampleFields;
		}

		/// <summary>
		/// returns the genotype flag
		/// </summary>
		public string[] GetPairEndReadCounts()
		{
			if (_tmp.FormatIndices.PR == null) return null;
			var pairEndReadCounts = _tmp.SampleColumns[_tmp.FormatIndices.PR.Value].Split(',');
			
			return pairEndReadCounts.Length == 0 ? null : pairEndReadCounts;
		}

		public string[] GetSplitReadCounts()
		{
			if (_tmp.FormatIndices.SR == null) return null;
			var splitReadCounts = _tmp.SampleColumns[_tmp.FormatIndices.SR.Value].Split(',');

			return splitReadCounts.Length == 0 ? null : splitReadCounts;
		}
	}
}