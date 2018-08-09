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
    }
}
