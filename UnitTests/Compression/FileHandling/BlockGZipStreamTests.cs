using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Compression.FileHandling;
using ErrorHandling.Exceptions;
using UnitTests.TestUtilities;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.Compression.FileHandling
{
    public sealed class BlockGZipStreamTests : RandomFileBase
    {
        #region members

        private readonly byte[] _expectedDecompressedBuffer;

        #endregion

        public BlockGZipStreamTests()
        {
            // TODO: Fix fragile constructor
            _expectedDecompressedBuffer = GrabBytes(ResourceUtilities.GetReadStream(Resources.TopPath("HelloWorld_original.dat")));
        }

        [Fact]
		public void FileIO()
        {
            var observedDecompressedBuffer = new byte[_expectedDecompressedBuffer.Length];
            string randomPath = GetRandomPath();

            // compress the data
            long observedPosition;

            using (var writer = new BlockGZipStream(FileUtilities.GetCreateStream(randomPath), CompressionMode.Compress))
            {
                writer.Write(_expectedDecompressedBuffer, 0, _expectedDecompressedBuffer.Length);
                observedPosition = writer.Position;

                var exception = Record.Exception(() =>
                {
                    var buffer = new byte[10];
                    // ReSharper disable once AccessToDisposedClosure
                    writer.Read(buffer, 0, 1);
                });

                Assert.NotNull(exception);
                Assert.IsType<CompressionException>(exception);
            }

            const long expectedPosition = 979042574;
            Assert.Equal(expectedPosition, observedPosition);

            // decompress the data
            using (var reader = new BlockGZipStream(FileUtilities.GetReadStream(randomPath), CompressionMode.Decompress))
            {
                reader.Read(observedDecompressedBuffer, 0, _expectedDecompressedBuffer.Length);

                var exception = Record.Exception(() =>
                {
                    var buffer = new byte[10];
                    // ReSharper disable once AccessToDisposedClosure
                    reader.Write(buffer, 0, 1);
                });

                Assert.NotNull(exception);
                Assert.IsType<CompressionException>(exception);
            }

            Assert.Equal(_expectedDecompressedBuffer, observedDecompressedBuffer);
        }

        [Fact]
        public void InvalidHeader()
        {
            const string dummyString = "The quick brown fox jumped over the lazy dog.";

            using (var ms          = new MemoryStream())
            using (var truncatedMs = new MemoryStream())
            {
                using (var writer = new StreamWriter(ms, Encoding.ASCII, 4096, true))
                {
                    writer.WriteLine(dummyString);
                }

                var observedCompressedBuffer = ms.ToArray();
                truncatedMs.Write(ms.ToArray(), 0, 17);

                ms.Seek(0, SeekOrigin.Begin);
                truncatedMs.Seek(0, SeekOrigin.Begin);

                // attempt to decompress the data
                Assert.Throws<CompressionException>(delegate
                {
                    using (var reader = new BlockGZipStream(ms, CompressionMode.Decompress, true))
                    {
                        reader.Read(observedCompressedBuffer, 0, observedCompressedBuffer.Length);
                    }
                });

                Assert.Throws<CompressionException>(delegate
                {
                    using (var reader = new BlockGZipStream(truncatedMs, CompressionMode.Decompress, true))
                    {
                        reader.Read(observedCompressedBuffer, 0, observedCompressedBuffer.Length);
                    }
                });
            }
        }

        [Fact]
        public void NullStream()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                using (new BlockGZipStream(null, CompressionMode.Decompress))
                {
                }
            });
        }

        [Fact]
        public void NotImplementedMethods()
        {
            using (var ms = new MemoryStream())
            {
                // ReSharper disable AccessToDisposedClosure
                using (var writer = new BlockGZipStream(ms, CompressionMode.Compress, true))
                {
                    Assert.Throws<NotSupportedException>(delegate
                    {
                        // ReSharper disable once UnusedVariable
                        long len = writer.Length;
                    });

                    Assert.Throws<NotSupportedException>(delegate { writer.SetLength(10); });

                    Assert.Throws<NotSupportedException>(delegate { writer.Seek(0, SeekOrigin.Begin); });
                }
                // ReSharper restore AccessToDisposedClosure
            }
        }

        [Fact]
        public void StreamIO()
        {
            byte[] observedCompressedBuffer;
            var observedDecompressedBuffer = new byte[_expectedDecompressedBuffer.Length];

            using (var ms = new MemoryStream())
            {
                // compress the data
                using (var writer = new BlockGZipStream(ms, CompressionMode.Compress, true, 9))
                {
                    Assert.Throws<CompressionException>(delegate
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        writer.Read(observedDecompressedBuffer, 0, 1);
                    });

                    Assert.True(writer.CanWrite);
                    Assert.False(writer.CanRead);
                    Assert.False(writer.CanSeek);

                    writer.Write(_expectedDecompressedBuffer, 0, _expectedDecompressedBuffer.Length);
                }

                observedCompressedBuffer = ms.ToArray();
                ms.Seek(0, SeekOrigin.Begin);

                // decompress the data
                using (var reader = new BlockGZipStream(ms, CompressionMode.Decompress))
                {
                    Assert.Throws<CompressionException>(delegate
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        reader.Write(_expectedDecompressedBuffer, 0, 1);
                    });

                    Assert.False(reader.CanWrite);
                    Assert.True(reader.CanRead);
                    Assert.True(reader.CanSeek);

                    reader.Read(observedDecompressedBuffer, 0, _expectedDecompressedBuffer.Length);
                }
            }

            Assert.Equal(_expectedDecompressedBuffer, observedDecompressedBuffer);
            Assert.Equal(9629, observedCompressedBuffer.Length);
        }

        [Fact]
        public void StreamTypeMismatch()
        {
            string randomPath = GetRandomPath();

            using (var writeStream = new FileStream(randomPath, FileMode.Create, FileAccess.Write))
            {
                Assert.Throws<CompressionException>(delegate
                {
                    // ReSharper disable once AccessToDisposedClosure
                    using (new BlockGZipStream(writeStream, CompressionMode.Decompress))
                    {
                    }
                });
            }

            using (var readStream = FileUtilities.GetReadStream(randomPath))
            {
                Assert.Throws<CompressionException>(delegate
                {
                    // ReSharper disable once AccessToDisposedClosure
                    using (new BlockGZipStream(readStream, CompressionMode.Compress))
                    {
                    }
                });
            }
        }

        [Theory]
        [InlineData(650*1024)]
        [InlineData(65*1024)]
        [InlineData(1024)]
        public void VariableDataLength(int numBytesToBeWritten)
        {
            using (var ms = new MemoryStream())
            {
                // compress our data
                using (var writer = new StreamWriter(new BlockGZipStream(ms, CompressionMode.Compress, true)))
                {
                    int currentIndex = 1;
                    int numBytes = 0;

                    while (true)
                    {
                        string s = $"Hello World {currentIndex}";
                        writer.WriteLine(s);
                        currentIndex++;
                        numBytes += s.Length;
                        if (numBytes > numBytesToBeWritten) break;
                    }
                }

                ms.Seek(0, SeekOrigin.Begin);

                // decompress our data
                using (var reader = new StreamReader(new BlockGZipStream(ms, CompressionMode.Decompress)))
                {
                    int index = 1;

                    while (true)
                    {
                        string expected = $"Hello World {index}";
                        index++;

                        string observed = reader.ReadLine();
                        if (observed == null) break;
                        Assert.Equal(expected, observed);
                    }
                }
            }
        }

        [Fact]
        public void EndOfFile()
        {
            using (var ms = new MemoryStream())
            {
                var writeBuffer = ByteUtilities.GetRandomBytes(100);
                var readBuffer = new byte[60];

                using (var bgzipStream = new BlockGZipStream(ms, CompressionMode.Compress, true))
                {
                    bgzipStream.Write(writeBuffer, 0, writeBuffer.Length);
                }

                ms.Position = 0;

                using (var bgzipStream = new BlockGZipStream(ms, CompressionMode.Decompress))
                {
                    int numBytesRead = bgzipStream.Read(readBuffer, 0, 0);
                    Assert.Equal(0, numBytesRead);

                    numBytesRead = bgzipStream.Read(readBuffer, 0, readBuffer.Length);
                    Assert.Equal(readBuffer.Length, numBytesRead);

                    numBytesRead = bgzipStream.Read(readBuffer, 0, readBuffer.Length);
                    Assert.Equal(writeBuffer.Length - readBuffer.Length, numBytesRead);

                    numBytesRead = bgzipStream.Read(readBuffer, 0, readBuffer.Length);
                    Assert.Equal(0, numBytesRead);
                }
            }
        }

        [Fact]
        public void ReadBlockCorrupted()
        {
            using (var ms          = new MemoryStream())
            using (var truncatedMs = new MemoryStream())
            using (var corruptMs   = new MemoryStream())
            {
                using (var bgzipStream = new BlockGZipStream(ms, CompressionMode.Compress, true))
                using (var writer      = new StreamWriter(bgzipStream, Encoding.ASCII, 4096))
                {
                    writer.WriteLine("The quick brown fox jumped over the lazy dog.");
                }

                var compressedData = ms.ToArray();

                truncatedMs.Write(compressedData, 0, compressedData.Length - 10);
                truncatedMs.Position = 0;

                corruptMs.Write(compressedData, 0, BlockGZipStream.BlockGZipFormatCommon.BlockHeaderLength);
                corruptMs.Write(_expectedDecompressedBuffer, 0, _expectedDecompressedBuffer.Length);
                corruptMs.Position = 0;

                var readBuffer = new byte[60];

                Assert.Throws<CompressionException>(delegate
                {
                    using (var bgzipStream = new BlockGZipStream(truncatedMs, CompressionMode.Decompress))
                    {
                        bgzipStream.Read(readBuffer, 0, readBuffer.Length);
                    }
                });

                Assert.Throws<CompressionException>(delegate
                {
                    using (var bgzipStream = new BlockGZipStream(corruptMs, CompressionMode.Decompress))
                    {
                        bgzipStream.Read(readBuffer, 0, readBuffer.Length);
                    }
                });
            }
        }

        [Fact]
        public void DoubleDispose()
        {
            using (var ms = new MemoryStream())
            {
                var bgzipStream = new BlockGZipStream(ms, CompressionMode.Compress);
                bgzipStream.Dispose();
                bgzipStream.Dispose();
            }
        }

        private static byte[] GrabBytes(Stream s)
        {
            byte[] buffer;

            using (var ms = new MemoryStream())
            {
                s.CopyTo(ms);
                buffer = ms.ToArray();
            }

            return buffer;
        }
    }
}