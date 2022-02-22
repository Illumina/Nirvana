using System.IO;
using Cache.Index;
using Cache.IO;
using Xunit;

namespace UnitTests.Cache.IO;

public sealed class CacheIndexWriterTests
{
    [Fact]
    public void Write_EndToEnd_ExpectedResults()
    {
        const int expectedFilePairId = 123;

        var chr1BinPositions = new[]
        {
            new BinPosition(3, 123),
            new BinPosition(9, 456)
        };

        var chr3BinPositions = new[]
        {
            new BinPosition(2, 789),
            new BinPosition(7, 1230)
        };

        var indexReferences = new[]
        {
            new IndexReference(0, 1000, chr1BinPositions),
            new IndexReference(2, 3000, chr3BinPositions)
        };

        var expectedCacheIndex = new CacheIndex(indexReferences);

        using var ms = new MemoryStream();
        using (var writer = new CacheIndexWriter(ms, expectedFilePairId, true))
        {
            writer.Write(expectedCacheIndex);
        }

        ms.Position = 0;

        CacheIndex actualCacheIndex;
        int        actualFilePairId;

        using (var reader = new CacheIndexReader(ms))
        {
            actualCacheIndex = reader.GetCacheIndex();
            actualFilePairId = reader.FilePairId;
        }

        Assert.Equal(expectedCacheIndex, actualCacheIndex);
        Assert.Equal(expectedFilePairId, actualFilePairId);
    }
}