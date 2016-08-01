using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("Chromosome 1 collection")]
    public sealed class JsonAnnotationTests
    {
        [Fact]
        public void ClinVarMixedFormatConsequence()
        {
            var s = @"Thyroid_cancer\x2c_follicular";
            s = SupplementaryAnnotation.ConvertMixedFormatString(s);
            Assert.Equal("Thyroid_cancer,_follicular", s);
        }

        [Fact]
        public void GetUnknownDesiredReferenceName()
        {
            const string expectedChromosomeName = "O";
            var chromosomeRenamer = AnnotationLoader.Instance.ChromosomeRenamer;

            string observedUcscReferenceName = chromosomeRenamer.GetUcscReferenceName(expectedChromosomeName);
            Assert.Equal(expectedChromosomeName, observedUcscReferenceName);

            string observedEnsemblReferenceName = chromosomeRenamer.GetEnsemblReferenceName(expectedChromosomeName);
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
    }
}