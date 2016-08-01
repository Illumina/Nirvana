using UnitTests.Utilities;
using Xunit;

namespace UnitTests.Interface
{
	[Collection("Chromosome 1 collection")]
	public sealed class CustomIntervalTest
	{
		[Fact]
		public void BasicCustomIntervalOutput()
		{
		    var customIntervals = ResourceUtilities.GetCustomIntervals("chr1_IcslIntervals_69090_69091.nci");
		    var annotationSource = ResourceUtilities.GetAnnotationSource("ENST00000546909_chr14_Ensembl84.ndb");
		    annotationSource.AddCustomIntervals(customIntervals);

		    var annotatedVariant = DataUtilities.GetVariant(annotationSource,
		        "chr1	69092	.	T	C	.	LowGQX;HighDPFRatio	END=10244;BLOCKAVG_min30p3a	GT:GQX:DP:DPF	.:.:0:1");
            Assert.NotNull(annotatedVariant);

            Assert.Contains("IcslInterval", annotatedVariant.ToString());
		}
	}
}
