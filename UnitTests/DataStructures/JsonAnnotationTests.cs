using System.Collections.Generic;
using UnitTests.Fixtures;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.Utilities;
using Xunit;
using SupplementaryAnnotationUtilities = VariantAnnotation.DataStructures.SupplementaryAnnotations.SupplementaryAnnotationUtilities;

namespace UnitTests.DataStructures
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
        public void ClinVarMixedFormatConsequence()
        {
            var s = @"Thyroid_cancer\x2c_follicular";
            s = SupplementaryAnnotationUtilities.ConvertMixedFormatString(s);
            Assert.Equal("Thyroid_cancer,_follicular", s);
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
        public void GenotypeIndexes()
        {
            var genotypeIndicies = new List<int>();

            // 0/0
            genotypeIndicies.Clear();
            VariantFeature.GetGenotypeIndices("0/0", genotypeIndicies);
            Assert.Equal(2, genotypeIndicies.Count);
            Assert.Equal(0, genotypeIndicies[0]);
            Assert.Equal(0, genotypeIndicies[1]);

            // 0/1
            genotypeIndicies.Clear();
            VariantFeature.GetGenotypeIndices("0/1", genotypeIndicies);
            Assert.Equal(2, genotypeIndicies.Count);
            Assert.Equal(0, genotypeIndicies[0]);
            Assert.Equal(1, genotypeIndicies[1]);

            // 1/1
            genotypeIndicies.Clear();
            VariantFeature.GetGenotypeIndices("1/1", genotypeIndicies);
            Assert.Equal(2, genotypeIndicies.Count);
            Assert.Equal(1, genotypeIndicies[0]);
            Assert.Equal(1, genotypeIndicies[1]);

            // 1/2
            genotypeIndicies.Clear();
            VariantFeature.GetGenotypeIndices("1/2", genotypeIndicies);
            Assert.Equal(2, genotypeIndicies.Count);
            Assert.Equal(1, genotypeIndicies[0]);
            Assert.Equal(2, genotypeIndicies[1]);

            // 0
            genotypeIndicies.Clear();
            VariantFeature.GetGenotypeIndices("0", genotypeIndicies);
            Assert.Equal(1, genotypeIndicies.Count);
            Assert.Equal(0, genotypeIndicies[0]);

            // 1
            genotypeIndicies.Clear();
            VariantFeature.GetGenotypeIndices("1", genotypeIndicies);
            Assert.Equal(1, genotypeIndicies.Count);
            Assert.Equal(1, genotypeIndicies[0]);

            // 2|3
            genotypeIndicies.Clear();
            VariantFeature.GetGenotypeIndices("2|3", genotypeIndicies);
            Assert.Equal(2, genotypeIndicies.Count);
            Assert.Equal(2, genotypeIndicies[0]);
            Assert.Equal(3, genotypeIndicies[1]);

            // ./0
            genotypeIndicies.Clear();
            VariantFeature.GetGenotypeIndices("./0", genotypeIndicies);
            Assert.Equal(0, genotypeIndicies.Count);

            // ./.
            genotypeIndicies.Clear();
            VariantFeature.GetGenotypeIndices("./.", genotypeIndicies);
            Assert.Equal(0, genotypeIndicies.Count);

            // .
            genotypeIndicies.Clear();
            VariantFeature.GetGenotypeIndices(".", genotypeIndicies);
            Assert.Equal(0, genotypeIndicies.Count);

            // bob
            genotypeIndicies.Clear();
            VariantFeature.GetGenotypeIndices("bob", genotypeIndicies);
            Assert.Equal(0, genotypeIndicies.Count);
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
			var annotatedVariant = DataUtilities.GetVariant(annotationSource, "chr1	10168	.	C	CT	.	LowGQX	.	GT:GQ:GQX:DPI:AD:ADF:ADR:PL:DQ	0/0:3:3:1:1,0:1,0:0,0:0,3,23:93	0/1:63:6:10:3,5:3,5:0,0:183,0,60:.	0/1:3:0:0:0,0:0,0:0,0:0,0,0:.");

            const string expectedSample = "\"genotype\":\"0/0\",\"deNovoQuality\":93";
            var observedJsonLine = annotatedVariant.ToString();
			Assert.Contains(expectedSample, observedJsonLine);
		}
    }
}