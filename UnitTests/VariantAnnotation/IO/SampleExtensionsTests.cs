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
            const string expectedGenotype             = "\"genotype\":\"1/2\"";
            const string expectedVariantFreq          = "\"variantFreq\":0.75";
            const string expectedTotaldepth           = "\"totalDepth\":34";
            const string expectedGenotypeQuality      = "\"genotypeQuality\":98";
            const string expectedCopyNumber           = "\"copyNumber\":3";
            const string expectedAlleleDepths         = "\"alleleDepths\":[23,34]";
            const string expectedFailedFilter         = "\"failedFilter\":true";
            const string expectedSplitReadCounts      = "\"splitReadCounts\":[10,15]";
            const string expectedPairedEndReadCounts  = "\"pairedEndReadCounts\":[8,14]";
            const string expectedLossOfHeterozygosity = "\"lossOfHeterozygosity\":true";
            const string expectedDeNovoQuality        = "\"deNovoQuality\":97";

            var sample = new Sample("1/2", 98, 0.75, 34, new[] { 23, 34 }, true, 3, true, 97, new[] { 10, 15 },
                new[] { 8, 14 }, null, null);

            var observedResult = sample.GetJsonString();

            Assert.Contains(expectedGenotype,             observedResult);
            Assert.Contains(expectedVariantFreq,          observedResult);
            Assert.Contains(expectedTotaldepth,           observedResult);
            Assert.Contains(expectedGenotypeQuality,      observedResult);
            Assert.Contains(expectedCopyNumber,           observedResult);
            Assert.Contains(expectedAlleleDepths,         observedResult);
            Assert.Contains(expectedFailedFilter,         observedResult);
            Assert.Contains(expectedSplitReadCounts,      observedResult);
            Assert.Contains(expectedPairedEndReadCounts,  observedResult);
            Assert.Contains(expectedLossOfHeterozygosity, observedResult);
            Assert.Contains(expectedDeNovoQuality,        observedResult);
        }
    }
}
