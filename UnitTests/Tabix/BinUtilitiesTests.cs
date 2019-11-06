using System.Collections.Generic;
using System.Linq;
using Tabix;
using Xunit;

namespace UnitTests.Tabix
{
    public sealed class BinUtilitiesTests
    {
        [Fact]
        public void BottomBin_Nominal()
        {
            int observedResults = BinUtilities.BottomBin(12517);
            Assert.Equal(7836, observedResults);
        }

        [Fact]
        public void ConvertPositionToBin_Nominal()
        {
            int observedResults = BinUtilities.ConvertPositionToBin(26699126);
            Assert.Equal(6310, observedResults);
        }

        [Fact]
        public void OverlappingBinsWithVariants_EndBeforeBegin_ReturnEmptyList()
        {
            IEnumerable<int> results = BinUtilities.OverlappingBinsWithVariants(20, 10, null);
            Assert.Empty(results);
        }

        [Fact]
        public void OverlappingBinsWithVariants_EndBeyondMaxRefLen_CorrectEnd()
        {
            const int expectedBinId = 6310;

            var idToChunks = new Dictionary<int, Interval[]>
            {
                [expectedBinId] = new[] { new Interval(1, 1) }
            };

            List<int> results = BinUtilities.OverlappingBinsWithVariants(10, int.MaxValue, idToChunks).ToList();
            Assert.Single(results);
            Assert.Equal(expectedBinId, results[0]);
        }
    }
}
