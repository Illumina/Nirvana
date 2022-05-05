using SAUtils.DataStructures;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.SAUtils.gnomAD;

public sealed class GnomadSvItemTests
{
    [Fact]
    public void TestGnomadSvItem()
    {
        var gnomadSvItem = new GnomadSvItem(ChromosomeUtilities.Chr1, "");

        Assert.Equal("",                                                                       gnomadSvItem.InputLine);
        Assert.Equal("\"chromosome\":\"1\",\"begin\":0,\"end\":0,\"variantType\":\"unknown\"", gnomadSvItem.GetJsonString());
    }
}