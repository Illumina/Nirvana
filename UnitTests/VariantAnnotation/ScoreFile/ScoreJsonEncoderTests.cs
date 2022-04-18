using VariantAnnotation.GenericScore;
using Xunit;

namespace UnitTests.VariantAnnotation.ScoreFile;

public sealed class ScoreJsonEncoderTests
{
    [Fact]
    public void TestJsonRepresentation()
    {
        var scoreJsonEncoder = new ScoreJsonEncoder("Test", "TestSubKey");
        
        Assert.Equal(
            "\"TestSubKey\":1",
            new ScoreJsonEncoder("Test", "TestSubKey").JsonRepresentation(1));
        Assert.Equal(
            "1",
            new ScoreJsonEncoder("Test", null).JsonRepresentation(1));
    }
}