using UnitTests.Utilities;
using Xunit;

namespace UnitTests.CustomInterval
{
    [Collection("Chromosome 1 collection")]
    public sealed class CustomIntervalOutput
    {
        [Fact]
        public void BasicCustomIntervalOutput()
        {
            var customIntervals  = ResourceUtilities.GetCustomIntervals("chr1_IcslIntervals_69090_69091.nci");
            var annotationSource = ResourceUtilities.GetAnnotationSource("ENST00000483270_chr1_Ensembl84.ndb");
            annotationSource.AddCustomIntervals(customIntervals);

            var annotatedVariant = DataUtilities.GetVariant(annotationSource,
                "chr1	69092	.	T	C	.	LowGQX;HighDPFRatio	END=10244;BLOCKAVG_min30p3a	GT:GQX:DP:DPF	.:.:0:1");
            Assert.NotNull(annotatedVariant);

            const string expectedJson = "{\"altAllele\":\"C\",\"refAllele\":\"T\",\"begin\":69092,\"chromosome\":\"chr1\",\"end\":69092,\"variantType\":\"SNV\",\"vid\":\"1:69092:C\",\"IcslIntervals\":[{\"Start\":69091,\"End\":70008,\"gene\":\"OR4F5\",\"assesment\":\"Some_evidence_of_constraint\",\"score\":0.0,\"exacScore\":3.60208899915}]}";
            var observedJson = JsonUtilities.GetFirstAlleleJson(annotatedVariant);
            Assert.Equal(expectedJson, observedJson);
        }
    }
}
