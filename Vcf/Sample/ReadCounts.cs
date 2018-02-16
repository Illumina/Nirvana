
namespace Vcf.Sample
{
    internal static class ReadCounts
	{
		public static int[] GetPairEndReadCounts(IntermediateSampleFields intermediateSampleFields)
		{
			if (intermediateSampleFields.FormatIndices.PR == null) return null;
			var readCounts = intermediateSampleFields.SampleColumns[intermediateSampleFields.FormatIndices.PR.Value].Split(',');

		    var pairEndReadCounts = new int[readCounts.Length];
		    for (int i = 0; i < pairEndReadCounts.Length; i++)
		    {
		        if (!int.TryParse(readCounts[i], out var num)) return null;
		        pairEndReadCounts[i] = num;
		    }

		    return pairEndReadCounts;
        }

		public static int[] GetSplitReadCounts(IntermediateSampleFields intermediateSampleFields)
		{
			if (intermediateSampleFields.FormatIndices.SR == null) return null;
			var splitReadCounts = intermediateSampleFields.SampleColumns[intermediateSampleFields.FormatIndices.SR.Value].Split(',');

		    var splitReads = new int[splitReadCounts.Length];
		    for (int i = 0; i < splitReads.Length; i++)
		    {
		        if (!int.TryParse(splitReadCounts[i], out var num)) return null;
		        splitReads[i] = num;
		    }

			return splitReads;
		}
	}
}