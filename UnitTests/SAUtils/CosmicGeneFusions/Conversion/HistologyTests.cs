using System.Collections.Generic;
using System.IO;
using SAUtils.CosmicGeneFusions.Conversion;
using Xunit;

namespace UnitTests.SAUtils.CosmicGeneFusions.Conversion
{
    public sealed class HistologyTests
    {
        private readonly HashSet<RawCosmicGeneFusion> _fusionEntries = new()
        {
            new RawCosmicGeneFusion(10, 0, null, null, "carcinoma", "ductal carcinoma",           null, 0),
            new RawCosmicGeneFusion(20, 0, null, null, "carcinoma", "ductal carcinoma",           null, 0),
            new RawCosmicGeneFusion(30, 0, null, null, "carcinoma", "NS",                         null, 0),
            new RawCosmicGeneFusion(40, 0, null, null, "carcinoma", "signet ring adenocarcinoma", null, 0)
        };

        [Fact]
        public void GetCounts_ExpectedResults()
        {
            const int     numSamples   = 4;
            CosmicCount[] actualCounts = Histology.GetCounts(_fusionEntries, numSamples);

            Assert.Equal(3, actualCounts.Length);

            CosmicCount actualCount = actualCounts[0];
            Assert.Equal("ductal carcinoma", actualCount.name);
            Assert.Equal(2,                  actualCount.numSamples);

            actualCount = actualCounts[1];
            Assert.Equal("carcinoma", actualCount.name);
            Assert.Equal(1,           actualCount.numSamples);

            actualCount = actualCounts[2];
            Assert.Equal("signet ring adenocarcinoma", actualCount.name);
            Assert.Equal(1,                            actualCount.numSamples);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        public void GetCounts_WrongSampleCount_ThrowException(int numSamples)
        {
            Assert.Throws<InvalidDataException>(delegate { Histology.GetCounts(_fusionEntries, numSamples); });
        }
    }
}