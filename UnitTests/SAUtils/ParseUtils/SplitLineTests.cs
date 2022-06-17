using SAUtils.ParseUtils;
using Xunit;

namespace UnitTests.SAUtils.ParseUtils;

public sealed class SplitLineTests
{
    [Theory]
    [InlineData("SomeString\tAnotherString", 0, "SomeString")]
    [InlineData("SomeString\tAnotherString", 1, "AnotherString")]
    [InlineData("\tAnotherString",           0, "")]
    [InlineData("\tAnotherString",           1, "AnotherString")]
    [InlineData("SomeString\t",              1, "")]
    [InlineData("SomeString\t",              0, "SomeString")]
    [InlineData("\t",                        0, "")]
    [InlineData("",                          0, "")]
    public void TestGetString(string inputLine, int index, string expectedString)
    {
        var splitLine = new SplitLine(inputLine, '\t');
        Assert.Equal(expectedString, splitLine.GetString(index));
    }

    [Theory]
    [InlineData("SomeString\t1",   0, null)]
    [InlineData("SomeString\t1",   1, 1)]
    [InlineData("SomeString\t2.0", 1, 2)]
    [InlineData("\t1",             0, null)]
    [InlineData("\t1",             1, 1)]
    [InlineData("SomeString\t",    1, null)]
    [InlineData("SomeString\t",    0, null)]
    [InlineData("\t",              0, null)]
    [InlineData("",                0, null)]
    [InlineData("A1",              0, null)]
    [InlineData("-1",              0, -1)]
    public void TestParseInteger(string inputLine, int index, int? expectedInt)
    {
        var splitLine = new SplitLine(inputLine, '\t');
        Assert.Equal(expectedInt, splitLine.ParseInteger(index));
    }
    
    [Theory]
    [InlineData("SomeString\t1",   0, null)]
    [InlineData("SomeString\t1",   1, 1.0)]
    [InlineData("SomeString\t2.0", 1, 2.0)]
    [InlineData("\t1",             0, null)]
    [InlineData("\t1",             1, 1.0)]
    [InlineData("SomeString\t",    1, null)]
    [InlineData("SomeString\t",    0, null)]
    [InlineData("\t",              0, null)]
    [InlineData("",                0, null)]
    [InlineData("A1",              0, null)]
    [InlineData("-1",              0, -1.0)]
    public void TestParseDouble(string inputLine, int index, double? expectedDouble)
    {
        var splitLine = new SplitLine(inputLine, '\t');
        Assert.Equal(expectedDouble, splitLine.ParseDouble(index));
    }
}