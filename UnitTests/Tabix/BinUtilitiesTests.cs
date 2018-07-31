using Tabix;
using Xunit;

namespace UnitTests.Tabix
{
    public sealed class BinUtilitiesTests
    {
        [Fact]
        public void ConvertRegionToBinList_Nominal()
        {
            var expectedResults = new[] {0, 1, 12, 98, 788, 6310, 6311};
            var observedResults = BinUtilities.ConvertRegionToBinList(26699126, 26714126);
            Assert.Equal(expectedResults, observedResults);
        }

        [Fact]
        public void ConvertRegionToBinList_EndBeforeBegin()
        {
            var observedResults = BinUtilities.ConvertRegionToBinList(26714126, 26699126);
            Assert.Null(observedResults);
        }

        [Fact]
        public void ConvertRegionToBinList_LargeSpan()
        {
            var observedResults = BinUtilities.ConvertRegionToBinList(243186006, 536880912);
            Assert.Equal(20490, observedResults.Length);
        }

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
    }
}
