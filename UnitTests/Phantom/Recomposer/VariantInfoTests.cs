using System.Collections.Generic;
using Phantom.PositionCollections;
using Phantom.Recomposer;
using Xunit;

namespace UnitTests.Phantom.Recomposer
{
    public sealed class VariantInfoTests
    {
        [Fact]
        public void GetMnvFilterTag_SampleWithFailedMnv_MakredAsFailed()
        {
            var sampleFilterFailed = new[] { new List<bool> { true, false }};
            var variantInfo = new VariantInfo(null, null, null, null, null, sampleFilterFailed);

            Assert.Equal(VariantInfo.FailedFilterTag, variantInfo.GetMnvFilterTag());
        }

        [Fact]
        public void GetMnvFilterTag_PassedSampleOverrideFailedSample()
        {
            var sampleFilterFailed = new[] { new List<bool> { true, false }, new List<bool> { false, false } };
            var variantInfo = new VariantInfo(null, null, null, null, null, sampleFilterFailed);

            Assert.Equal("PASS", variantInfo.GetMnvFilterTag());
        }

        [Fact]
        public void UpdateSampleFilters_AsExpected()
        {
            var positionFilters = new[] { "PASS", "FAILED", "." };
            var sampleFilterFailed = new[] { new List<bool>(), new List<bool>() };
            var variantInfo = new VariantInfo(null, positionFilters, null, null, null, sampleFilterFailed);

            variantInfo.UpdateSampleFilters(new[] { 0, 2 }, new List<SampleHaplotype> { new SampleHaplotype(0, 0), new SampleHaplotype(1, 1) });
            variantInfo.UpdateSampleFilters(new[] { 0, 1, 2 }, new List<SampleHaplotype> { new SampleHaplotype(0, 1) });

            Assert.Equal(2, sampleFilterFailed.Length);
            Assert.Equal(2, sampleFilterFailed[0].Count);
            Assert.Single(sampleFilterFailed[1]);
            Assert.False(sampleFilterFailed[0][0]);
            Assert.True(sampleFilterFailed[0][1]);
            Assert.False(sampleFilterFailed[1][0]);
        }
    }
}