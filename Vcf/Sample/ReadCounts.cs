
using OptimizedCore;

namespace Vcf.Sample
{
    internal static class ReadCounts
	{
		public static int[] GetPairEndReadCounts(IntermediateSampleFields intermediateSampleFields)
		{
			if (intermediateSampleFields.FormatIndices.PR == null) return null;
			var readCounts = intermediateSampleFields.SampleColumns[intermediateSampleFields.FormatIndices.PR.Value].OptimizedSplit(',');

		    var pairEndReadCounts = new int[readCounts.Length];

		    for (var i = 0; i < pairEndReadCounts.Length; i++)
		    {
		        (int number, bool foundError) = readCounts[i].OptimizedParseInt32();
		        if (foundError) return null;
		        pairEndReadCounts[i] = number;
		    }

		    return pairEndReadCounts;
        }

		public static int[] GetSplitReadCounts(IntermediateSampleFields intermediateSampleFields)
		{
			if (intermediateSampleFields.FormatIndices.SR == null) return null;
			var splitReadCounts = intermediateSampleFields.SampleColumns[intermediateSampleFields.FormatIndices.SR.Value].OptimizedSplit(',');

		    var splitReads = new int[splitReadCounts.Length];

		    for (var i = 0; i < splitReads.Length; i++)
		    {
		        (int number, bool foundError) = splitReadCounts[i].OptimizedParseInt32();
		        if (foundError) return null;
		        splitReads[i] = number;
		    }

			return splitReads;
		}
	}
}