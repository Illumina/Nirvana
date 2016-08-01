using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using ErrorHandling.Exceptions;
using UnitTests.Utilities;
using VariantAnnotation.Compression;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.Compression
{
    public sealed class BGZipTests : RandomFileBase
    {
        public BGZipTests()
        {
            _expectedDecompressedBuffer = GrabBytes(ResourceUtilities.GetFileStream("HelloWorld_original.txt"));
        }

        [Fact]
        public void BGZipFile()
        {
            var observedDecompressedBuffer = new byte[_expectedDecompressedBuffer.Length];
            string randomPath = GetRandomPath();

            // compress the data
            long observedPosition;

            using (var writer = new BlockGZipStream(new FileStream(randomPath, FileMode.Create), CompressionMode.Compress))
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

            const long expectedPosition = 270;
            Assert.Equal(expectedPosition, observedPosition);

            // decompress the data
            using (var reader = new BlockGZipStream(FileUtilities.GetFileStream(randomPath), CompressionMode.Decompress))
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

            using (var ms = new MemoryStream())
            {
                using (var writer = new StreamWriter(ms, Encoding.ASCII, 4096, true))
                {
                    writer.WriteLine(dummyString);
                }

                var observedCompressedBuffer = ms.ToArray();
                ms.Seek(0, SeekOrigin.Begin);

                // attempt to decompress the data
                var exception = Record.Exception(() =>
                {
                    // ReSharper disable AccessToDisposedClosure
                    using (var reader = new BlockGZipStream(ms, CompressionMode.Decompress, true))
                    {
                        reader.Read(observedCompressedBuffer, 0, observedCompressedBuffer.Length);
                    }
                    // ReSharper restore AccessToDisposedClosure
                });
                
                Assert.NotNull(exception);
                Assert.IsType<CompressionException>(exception);
            }
        }

        [Fact]
        public void BGZipFileNotFound()
        {
            string randomPath = GetRandomPath();

            Assert.Throws<FileNotFoundException>(delegate
            {
                using (new BlockGZipStream(FileUtilities.GetFileStream(randomPath), CompressionMode.Decompress))
                {
                }
            });
        }

        [Fact]
        public void BGZipNotImplementedMethods()
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

                    Assert.Throws<NotSupportedException>(delegate { writer.Position = 10; });

                    Assert.Throws<NotSupportedException>(delegate { writer.SetLength(10); });

                    Assert.Throws<NotSupportedException>(delegate { writer.Seek(0, SeekOrigin.Begin); });
                }
                // ReSharper restore AccessToDisposedClosure
            }
        }

        [Fact]
        public void BGZipStream()
        {
            byte[] observedCompressedBuffer;
            var observedDecompressedBuffer = new byte[_expectedDecompressedBuffer.Length];

            using (var ms = new MemoryStream())
            {
                // compress the data
                using (var writer = new BlockGZipStream(ms, CompressionMode.Compress, true))
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

            Assert.Equal(15061, observedCompressedBuffer.Length);
            Assert.Equal(_expectedDecompressedBuffer, observedDecompressedBuffer);
        }

        [Fact]
        public void BGZipStreamTypeMismatch()
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

            using (var readStream = FileUtilities.GetFileStream(randomPath))
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
        public void BGZipTextTheory(int numBytesToBeWritten)
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

        #region members

        private readonly byte[] _expectedDecompressedBuffer;

        #endregion
    }
}