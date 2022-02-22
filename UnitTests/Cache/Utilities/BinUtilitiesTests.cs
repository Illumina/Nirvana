using Cache.Utilities;
using Xunit;

namespace UnitTests.Cache.Utilities;

public sealed class BinUtilitiesTests
{
    [Theory]
    [InlineData(1,          0)]
    [InlineData(524_288,    0)]
    [InlineData(1_048_576,  0)]
    [InlineData(1_048_577,  1)]
    [InlineData(1_572_864,  1)]
    [InlineData(2_097_152,  1)]
    [InlineData(2_097_153,  2)]
    [InlineData(17_301_504, 16)]
    public void GetBin_ExpectedResults(int position, byte expectedBin)
    {
        byte actualBin = BinUtilities.GetBin(position);
        Assert.Equal(expectedBin, actualBin);
    }
}