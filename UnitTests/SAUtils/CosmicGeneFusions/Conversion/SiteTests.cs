using System.Collections.Generic;
using System.IO;
using SAUtils.CosmicGeneFusions.Conversion;
using Xunit;

namespace UnitTests.SAUtils.CosmicGeneFusions.Conversion
{
    public sealed class SiteTests
    {
        private readonly HashSet<RawCosmicGeneFusion> _fusionEntries = new()
        {
            new RawCosmicGeneFusion(10, 0, "skin",        "ear",          null, null, null, 0),
            new RawCosmicGeneFusion(20, 0, "skin",        "NS",           null, null, null, 0),
            new RawCosmicGeneFusion(30, 0, "skin",        "ear",          null, null, null, 0),
            new RawCosmicGeneFusion(40, 0, "soft tissue", "blood vessel", null, null, null, 0)
        };

        [Fact]
        public void GetCounts_ExpectedResults()
        {
            const int     numSamples   = 4;
            CosmicCount[] actualCounts = Site.GetCounts(_fusionEntries, numSamples);

            Assert.Equal(3, actualCounts.Length);

            CosmicCount actualCount = actualCounts[0];
            Assert.Equal("skin (ear)", actualCount.name);
            Assert.Equal(2,            actualCount.numSamples);

            actualCount = actualCounts[1];
            Assert.Equal("skin", actualCount.name);
            Assert.Equal(1,      actualCount.numSamples);

            actualCount = actualCounts[2];
            Assert.Equal("soft tissue (blood vessel)", actualCount.name);
            Assert.Equal(1,                            actualCount.numSamples);
        }

        [Fact]
        public void GetCounts_TotalSampleCountTooHigh_ThrowException()
        {
            const int numSamples = 3;
            Assert.Throws<InvalidDataException>(delegate { Site.GetCounts(_fusionEntries, numSamples); });
        }
    }
}