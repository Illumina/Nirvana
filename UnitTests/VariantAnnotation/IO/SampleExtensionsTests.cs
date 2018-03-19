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
            var sample = new Sample("1/2", 98, new[] {0.34, 0.56}, 34, new[] {23, 34}, true, 3, true, 97,
                new[] {10, 15}, new[] {8, 14}, null, null, new[] {41, 49}, "-", new[] {6606, 6607}, new[] {3, 3},
                new[] {"Orphanet"}, new[] {"70"}, new [] { "-", "+" }, new[] {123, 456}, true, 12.345f, 56.67f);

            var observedResult = sample.GetJsonString();

            Assert.Contains("\"genotype\":\"1/2\"",               observedResult);
            Assert.Contains("\"variantFrequencies\":[0.34,0.56]", observedResult);
            Assert.Contains("\"totalDepth\":34",                  observedResult);
            Assert.Contains("\"genotypeQuality\":98",             observedResult);
            Assert.Contains("\"copyNumber\":3",                   observedResult);
            Assert.Contains("\"alleleDepths\":[23,34]",           observedResult);
            Assert.Contains("\"failedFilter\":true",              observedResult);
            Assert.Contains("\"splitReadCounts\":[10,15]",        observedResult);
            Assert.Contains("\"pairedEndReadCounts\":[8,14]",     observedResult);
            Assert.Contains("\"lossOfHeterozygosity\":true",      observedResult);
            Assert.Contains("\"deNovoQuality\":97",               observedResult);

            Assert.Contains("\"mpileupAlleleDepths\":[41,49]",                 observedResult);
            Assert.Contains("\"silentCarrierHaplotype\":\"-\"",                observedResult);
            Assert.Contains("\"paralogousEntrezGeneIds\":[6606,6607]",         observedResult);
            Assert.Contains("\"paralogousGeneCopyNumbers\":[3,3]",             observedResult);
            Assert.Contains("\"diseaseClassificationSources\":[\"Orphanet\"]", observedResult);
            Assert.Contains("\"diseaseIds\":[\"70\"]",                         observedResult);
            Assert.Contains("\"diseaseAffectedStatuses\":[\"-\",\"+\"]",       observedResult);
            Assert.Contains("\"proteinAlteringVariantPositions\":[123,456]",   observedResult);
            Assert.Contains("\"isCompoundHetCompatible\":true",                observedResult);
            Assert.Contains("\"artifactAdjustedQualityScore\":12.3",           observedResult);
            Assert.Contains("\"likelihoodRatioQualityScore\":56.7",            observedResult);
        }
    }
}
