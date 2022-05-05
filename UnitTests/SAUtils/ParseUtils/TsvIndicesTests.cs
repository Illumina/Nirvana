using SAUtils.ParseUtils;
using Xunit;

namespace UnitTests.SAUtils.ParseUtils;

public class TsvIndicesTests
{
    [Theory]
    [InlineData(0, 1)]
    public void TestTsvIndices(ushort chromosomeIndex, ushort startIndex)
    {
        var tsvIndices = new TsvIndices()
        {
            Chromosome = chromosomeIndex,
            Start = startIndex
        };
        
        Assert.Equal(tsvIndices.Chromosome, chromosomeIndex);
        Assert.Equal(tsvIndices.Start, startIndex);
        Assert.Equal(tsvIndices.SvType, ushort.MaxValue);
    }
}