using System.IO;
using Compression.FileHandling;
using ErrorHandling.Exceptions;
using Xunit;

namespace UnitTests.Compression.FileHandling
{
    public sealed class BlockHeaderTests
    {
        [Fact]
        public void ReadAndWrite()
        {
            const int expectedNumUncompressedBytes = 100;
            const int expectedNumCompressedBytes   = 50;

            var header = new BlockHeader
            {
                NumUncompressedBytes = expectedNumUncompressedBytes,
                NumCompressedBytes   = expectedNumCompressedBytes
            };

            using (var ms = new MemoryStream())
            {
                header.Write(ms);

                ms.Seek(0, SeekOrigin.Begin);

                header.NumUncompressedBytes = -1;
                header.NumCompressedBytes   = -1;

                header.Read(ms);
            }

            Assert.Equal(expectedNumUncompressedBytes, header.NumUncompressedBytes);
            Assert.Equal(expectedNumCompressedBytes, header.NumCompressedBytes);
        }

        [Fact]
        public void SizeMismatch()
        {
            using (var ms = new MemoryStream())
            {
                var array = new byte[10];
                ms.Write(array, 0, array.Length);

                ms.Seek(0, SeekOrigin.Begin);

                var header = new BlockHeader();
                // ReSharper disable once AccessToDisposedClosure
                Assert.Throws<IOException>(delegate { header.Read(ms); });
            }
        }

        [Fact]
        public void WrongHeaderId()
        {
            using (var ms = new MemoryStream())
            {
                var array = new byte[12];
                ms.Write(array, 0, array.Length);

                ms.Seek(0, SeekOrigin.Begin);

                var header = new BlockHeader();
                // ReSharper disable once AccessToDisposedClosure
                Assert.Throws<CompressionException>(delegate { header.Read(ms); });
            }
        }
    }
}
