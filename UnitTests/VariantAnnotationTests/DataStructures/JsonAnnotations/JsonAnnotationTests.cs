using UnitTests.Fixtures;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.VariantAnnotationTests.DataStructures.JsonAnnotations
{
    [Collection("ChromosomeRenamer")]
    public sealed class JsonAnnotationTests
    {
        private readonly ChromosomeRenamer _renamer;

        /// <summary>
        /// constructor
        /// </summary>
        public JsonAnnotationTests(ChromosomeRenamerFixture fixture)
        {
            _renamer = fixture.Renamer;
        }

        [Fact]
        public void GetUnknownDesiredReferenceName()
        {
            const string expectedChromosomeName = "O";

            var observedUcscReferenceName = _renamer.GetUcscReferenceName(expectedChromosomeName);
            Assert.Equal(expectedChromosomeName, observedUcscReferenceName);

            var observedEnsemblReferenceName = _renamer.GetEnsemblReferenceName(expectedChromosomeName);
            Assert.Equal(expectedChromosomeName, observedEnsemblReferenceName);
        }

        [Fact]
        public void Sample()
        {
            var sampleEntry = new JsonSample
            {
                AlleleDepths = new[] { "92", "21" },
                VariantFrequency = "1",
                TotalDepth = "10",
                FailedFilter = true,
                GenotypeQuality = "790"
            };

            var observedJson = sampleEntry.ToString();

            const string expectedJson =
                "{\"variantFreq\":1,\"totalDepth\":10,\"genotypeQuality\":790,\"alleleDepths\":[92,21],\"failedFilter\":true}";
            Assert.Equal(expectedJson, observedJson);
        }

	    [Fact]
	    public void SampleDenovoQuality()
	    {
	        var annotationSource = DataUtilities.EmptyAnnotationSource;
	        var annotatedVariant = DataUtilities.GetVariant(annotationSource,
	            VcfUtilities.GetVcfVariant(
	                "chr1	10168	.	C	CT	.	LowGQX	.	GT:GQ:GQX:DPI:AD:ADF:ADR:PL:DQ	0/0:3:3:1:1,0:1,0:0,0:0,3,23:93	0/1:63:6:10:3,5:3,5:0,0:183,0,60:.	0/1:3:0:0:0,0:0,0:0,0:0,0,0:."));

            const string expectedSample = "\"genotype\":\"0/0\",\"deNovoQuality\":93";
            var observedJsonLine = annotatedVariant.ToString();
			Assert.Contains(expectedSample, observedJsonLine);
		}
    }
}