using System.IO;
using UnitTests.Fixtures;
using UnitTests.Mocks;
using UnitTests.Utilities;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.CustomInterval
{
    [Collection("ChromosomeRenamer")]
    public sealed class CustomIntervalOutput
    {
        private readonly ChromosomeRenamer _renamer;

        /// <summary>
        /// constructor
        /// </summary>
        public CustomIntervalOutput(ChromosomeRenamerFixture fixture)
        {
            _renamer = fixture.Renamer;
        }

        [Fact]
        public void BasicCustomIntervalOutput()
        {
            var customIntervalProvider = new MockCustomIntervalProvider(ResourceUtilities.GetReadStream(Resources.CustomIntervals("chr1_IcslIntervals_69090_69091.nci")), _renamer);
            var annotationSource = ResourceUtilities.GetAnnotationSource(DataUtilities.EmptyCachePrefix, null, null, customIntervalProvider);

            var annotatedVariant = DataUtilities.GetVariant(annotationSource,
                "chr1	69092	.	T	C	.	LowGQX;HighDPFRatio	END=10244;BLOCKAVG_min30p3a	GT:GQX:DP:DPF	.:.:0:1");
            Assert.NotNull(annotatedVariant);

            const string expectedJson = "{\"altAllele\":\"C\",\"refAllele\":\"T\",\"begin\":69092,\"chromosome\":\"chr1\",\"end\":69092,\"variantType\":\"SNV\",\"vid\":\"1:69092:C\",\"IcslIntervals\":[{\"Start\":69091,\"End\":70008,\"gene\":\"OR4F5\",\"assesment\":\"Some_evidence_of_constraint\",\"score\":0.0,\"exacScore\":3.60208899915}]}";
            var observedJson = JsonUtilities.GetFirstAlleleJson(annotatedVariant);
            Assert.Equal(expectedJson, observedJson);
        }
    }
}
