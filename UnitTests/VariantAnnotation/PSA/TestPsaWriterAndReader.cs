using Xunit;

namespace UnitTests.VariantAnnotation.PSA;

public sealed class TestPsaWriterAndReader
{
    [Fact]
    public void PsaReaderWriterTest()
    {
        using var reader = PsaTestUtilities.GetSiftPsaReader();
        Assert.Null(reader.GetScore(4, "TR_0004",   12, 'K').score);
        Assert.Null(reader.GetScore(0, "Trans-001", 12, 'K').score);
        Assert.Null(reader.GetScore(0, "Trans-001", 12, 'K').score);

        Assert.Equal((0.01, "deleterious - low confidence"), reader.GetScore(6, "NM_005228.3", 1, 'A'));
        Assert.Equal((0.23, "tolerated - low confidence"),   reader.GetScore(6, "NM_005228.3", 3, 'S'));
        
        Assert.Equal((0.0, "deleterious - low confidence"), reader.GetScore(9, "NM_020975.4", 1, 'C'));
        Assert.Equal((0.0, "deleterious"), reader.GetScore(10, "NM_001130442.2", 1, 'A'));
    }
}