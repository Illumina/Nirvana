using System.IO;
using VariantAnnotation.GenericScore;
using Xunit;

namespace UnitTests.SAUtils.GERP;

public sealed class GerpReaderTests
{
    /// <summary>
    /// This test is used to test backward compatibility with reader and writer.
    /// We do use schema versions to keep them in sync, but if one forgets to update
    /// the schema version, then the reader will fail.
    /// There are other tests that consider writing and reading in the same loop,
    /// however, in that case, a new code tests the writer and reader.
    /// Contrasting with this case, it will test the backward compatibility of the reader
    /// in case the reader code has a breaking change that prevents it from reading
    /// the old score files.
    /// </summary>
    [Fact]
    public void TestReadGerpData()
    {
        // This is the raw data from the files as byte array generated using wig file with one position
        // 1	12646	12647	0.298
        var indexStreamRaw = new byte[]
        {
            137, 78, 73, 82, 13, 10, 26, 10, 100, 25, 1, 0, 202, 250, 153, 145, 3, 135, 195, 225, 240, 2, 4, 71, 101, 114, 112, 8, 49, 49, 49, 49, 49,
            49, 49, 49, 128, 128, 188, 209, 129, 179, 218, 238, 4, 59, 80, 97, 116, 104, 111, 103, 101, 110, 105, 99, 105, 116, 121, 32, 115, 99, 111,
            114, 101, 115, 32, 111, 102, 32, 109, 105, 115, 115, 101, 110, 115, 101, 32, 118, 97, 114, 105, 97, 110, 116, 115, 32, 112, 114, 101, 100,
            105, 99, 116, 101, 100, 32, 98, 121, 32, 71, 101, 114, 112, 22, 1, 0, 1, 231, 98, 21, 83, 1, 0, 1, 0, 0, 223, 79, 141, 151, 110, 18,
            211, 63, 4, 103, 101, 114, 112, 5, 115, 99, 111, 114, 101, 1, 1, 78, 192, 132, 61
        };

        var dataStreamRaw = new byte[]
        {
            137, 78, 73, 82, 13, 10, 26, 10, 112, 23, 1, 0, 202, 250, 153, 145, 3, 135, 195, 225, 240, 40, 181, 47, 253, 160, 128, 132, 30, 0, 92, 0,
            0, 24, 0, 0, 255, 1, 0, 250, 255, 57, 24, 2, 2, 0, 16, 255, 2, 0, 16, 255, 2, 0, 16, 255, 2, 0, 16, 255, 2, 0, 16, 255, 2, 0, 16, 255, 2,
            0, 16, 255, 2, 0, 16, 255, 2, 0, 16, 255, 2, 0, 16, 255, 2, 0, 16, 255, 2, 0, 16, 255, 2, 0, 16, 255, 2, 0, 16, 255, 3, 36, 4, 255, 78,
            73, 82, 255
        };

        using (var dataStream = new MemoryStream(dataStreamRaw))
        using (var indexStream = new MemoryStream(indexStreamRaw))
        {
            var scoreReader = ScoreReader.Read(dataStream, indexStream);
            Assert.Equal(0.298, scoreReader.GetScore(0, 12647, "A"));
        }
    }
}