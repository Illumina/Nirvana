using VariantAnnotation.IO;
using Vcf.Sample;
using Xunit;

namespace UnitTests.VariantAnnotation.IO
{
    public sealed class SampleExtensionsTests
    {
        [Fact]
        public void GetJsonString_Nominal()
        {
            var sample = new Sample(new[] {23, 34}, 12.345f, 3, new[] {"-", "+"}, true, "1/2", 98, true, 56.67f,
                new[] {8, 14}, new[] {7, 4}, new[] {10, 15}, 34, new[] {0.34, 0.56});

            string observedResult = sample.GetJsonString();

            Assert.Contains("\"alleleDepths\":[23,34]",                  observedResult);
            Assert.Contains("\"artifactAdjustedQualityScore\":12.3",     observedResult);
            Assert.Contains("\"copyNumber\":3",                          observedResult);
            Assert.Contains("\"diseaseAffectedStatuses\":[\"-\",\"+\"]", observedResult);
            Assert.Contains("\"failedFilter\":true",                     observedResult);
            Assert.Contains("\"genotype\":\"1/2\"",                      observedResult);
            Assert.Contains("\"genotypeQuality\":98",                    observedResult);
            Assert.Contains("\"isDeNovo\":true",                         observedResult);
            Assert.Contains("\"likelihoodRatioQualityScore\":56.7",      observedResult);
            Assert.Contains("\"pairedEndReadCounts\":[8,14]",            observedResult);
            Assert.Contains("\"repeatUnitCounts\":[7,4]",                observedResult);
            Assert.Contains("\"splitReadCounts\":[10,15]",               observedResult);
            Assert.Contains("\"totalDepth\":34",                         observedResult);
            Assert.Contains("\"variantFrequencies\":[0.34,0.56]",        observedResult);
        }
    }
}
