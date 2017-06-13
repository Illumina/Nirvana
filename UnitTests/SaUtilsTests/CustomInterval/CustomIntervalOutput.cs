using UnitTests.Fixtures;
using UnitTests.Utilities;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.SaUtilsTests.CustomInterval
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
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix,
                Resources.CustomIntervals("chr1_69090_69091_IcslIntervals.nsa"),
                "chr1	69092	.	T	C	.	LowGQX;HighDPFRatio	END=10244;BLOCKAVG_min30p3a	GT:GQX:DP:DPF	.:.:0:1");
            Assert.NotNull(annotatedVariant);

            const string expectedJson = "{\"chromosome\":\"chr1\",\"refAllele\":\"T\",\"position\":69092,\"svEnd\":10244,\"filters\":[\"LowGQX\",\"HighDPFRatio\"],\"altAlleles\":[\"C\"],\"cytogeneticBand\":\"1p36.33\",\"samples\":[{\"totalDepth\":0}],\"IcslIntervals\":[{\"Start\":69091,\"End\":70008,\"gene\":\"OR4F5\",\"assesment\":\"Some_evidence_of_constraint\",\"score\":0.0,\"exacScore\":3.60208899915}]";

            var observedJson = annotatedVariant.ToString();
            Assert.Contains(expectedJson, observedJson);
        }
    }
}
