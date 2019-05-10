using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Compression.Algorithms;
using Compression.DataStructures;
using Compression.FileHandling;
using ErrorHandling.Exceptions;
using Genome;
using IO;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO.Caches;
using Xunit;

namespace UnitTests.Compression.FileHandling
{
    public sealed class BlockStreamTests
    {
        private const long NumTicks                   = 3;
        private const GenomeAssembly ExpectedAssembly = GenomeAssembly.hg19;
        private const string SmallString              = "Testing 123";
        private const string FinalString              = "Squeamish Ossifrage";

        private static readonly Random Random  = new Random(10);
        private static readonly Zstandard Zstd = new Zstandard(1);

        [Fact]
        public void BlockStream_EndToEnd()
        {
            string expectedString = GetRandomString(Block.DefaultSize + 10000);

            var customHeader = new DemoCustomHeader(-1, -1);
            var header = new DemoHeader(CacheConstants.Identifier, CacheConstants.SchemaVersion,
                CacheConstants.DataVersion, Source.Ensembl, NumTicks, ExpectedAssembly, customHeader);

            using (var ms = new MemoryStream())
            {                
                WriteBlockStream(Zstd, header, customHeader, ms, expectedString);
                ms.Position = 0;
                ReadFromBlockStream(Zstd, ms, expectedString);
            }
        }

        private static string GetRandomString(int length)
        {
            const string chars = " !\"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        public static byte[] GetRandomBytes(int numBytes)
        {
            var buffer = new byte[numBytes];
            using (var csp = RandomNumberGenerator.Create()) csp.GetBytes(buffer);
            return buffer;
        }

        // ReSharper disable once UnusedParameter.Local
        private static void ReadFromBlockStream(ICompressionAlgorithm compressionAlgorithm, Stream ms, string expectedRandomString)
        {
            // grab the header
            var header = DemoHeader.Read(ms);
            Assert.Equal(ExpectedAssembly, header.Assembly);

            using (var blockStream = new BlockStream(compressionAlgorithm, ms, CompressionMode.Decompress))
            using (var reader      = new ExtendedBinaryReader(blockStream))
            {
                CheckWriteException(blockStream);

                // sequential string check
                CheckString(reader, expectedRandomString);
                CheckString(reader, SmallString);
                CheckString(reader, FinalString);

                // random access string check
                blockStream.SetBlockPosition(header.Custom.FileOffset, header.Custom.InternalOffset);
                //reader.Reset();

                CheckString(reader, SmallString);
            }
        }

        // ReSharper disable once UnusedParameter.Local
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void CheckString(ExtendedBinaryReader reader, string expectedString)
        {
            string s = reader.ReadAsciiString();           
            Assert.NotNull(s);
            Assert.Equal(expectedString.Length, s.Length);
            Assert.Equal(expectedString, s);
        }

        private static void WriteBlockStream(ICompressionAlgorithm compressionAlgorithm, DemoHeader header,
            DemoCustomHeader customHeader, Stream ms, string s)
        {
            using (var blockStream  = new BlockStream(compressionAlgorithm, ms, CompressionMode.Compress, true))
            using (var writer       = new ExtendedBinaryWriter(blockStream))
            {
                CheckReadException(blockStream);

                blockStream.WriteHeader(header.Write);

                writer.WriteOptAscii(s);

                (customHeader.FileOffset, customHeader.InternalOffset) = blockStream.GetBlockPosition();
                Assert.Equal(customHeader.FileOffset, blockStream.Position);

                writer.WriteOptAscii(SmallString);
                blockStream.Flush();

                // this will be flushed during dispose
                writer.WriteOptAscii(FinalString);
            }
        }

        private static void CheckReadException(Stream writer)
        {
            var exception = Record.Exception(() =>
            {
                var buffer = new byte[10];
                // ReSharper disable once AccessToDisposedClosure
                writer.Read(buffer, 0, 1);
            });

            Assert.NotNull(exception);
            Assert.IsType<CompressionException>(exception);
        }

        private static void CheckWriteException(Stream reader)
        {
            var exception = Record.Exception(() =>
            {
                var buffer = new byte[10];
                // ReSharper disable once AccessToDisposedClosure
                reader.Write(buffer, 0, 1);
            });

            Assert.NotNull(exception);
            Assert.IsType<CompressionException>(exception);
        }

        [Fact]
        public void NullStream()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                using (new BlockStream(Zstd, null, CompressionMode.Decompress))
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
                using (var writer = new BlockStream(Zstd, ms, CompressionMode.Compress, true))
                {
                    Assert.Throws<NotSupportedException>(delegate
                    {
                        // ReSharper disable once UnusedVariable
                        long len = writer.Length;
                    });

                    Assert.Throws<NotSupportedException>(delegate { writer.SetLength(10); });

                    Assert.Throws<NotSupportedException>(delegate { writer.Seek(0, SeekOrigin.Begin); });

                    Assert.Throws<NotSupportedException>(delegate { writer.Position = 0; });
                }
                // ReSharper restore AccessToDisposedClosure
            }
        }

        [Fact]
        public void StreamTypeMismatch()
        {
            string randomPath = RandomPath.GetRandomPath();

            using (var writeStream = new FileStream(randomPath, FileMode.Create, FileAccess.Write))
            {
                Assert.Throws<ArgumentException>(delegate
                {
                    // ReSharper disable once AccessToDisposedClosure
                    using (new BlockStream(Zstd, writeStream, CompressionMode.Decompress))
                    {
                    }
                });
            }

            using (var readStream = FileUtilities.GetReadStream(randomPath))
            {
                Assert.Throws<ArgumentException>(delegate
                {
                    // ReSharper disable once AccessToDisposedClosure
                    using (new BlockStream(Zstd, readStream, CompressionMode.Compress))
                    {
                    }
                });
            }
        }

        [Fact]
        public void CanReadWriteSeek()
        {
            string randomPath = RandomPath.GetRandomPath();

            using (var writeStream = new FileStream(randomPath, FileMode.Create, FileAccess.Write))
            using (var blockStream = new BlockStream(Zstd, writeStream, CompressionMode.Compress))
            {
                Assert.False(blockStream.CanRead);
                Assert.True(blockStream.CanWrite);
                Assert.True(blockStream.CanSeek);
            }
        }

        [Fact]
        public void ValidateParameters()
        {
            using (var ms = new MemoryStream())
            {
                using (var blockStream = new BlockStream(Zstd, ms, CompressionMode.Compress))
                {
                    var buffer = new byte[10];

                    // ReSharper disable once AssignNullToNotNullAttribute
                    Assert.Throws<ArgumentNullException>(delegate       { blockStream.Write(null, 10, 10);   });
                    Assert.Throws<ArgumentOutOfRangeException>(delegate { blockStream.Write(buffer, -1, 10); });
                    Assert.Throws<ArgumentOutOfRangeException>(delegate { blockStream.Write(buffer, 10, -1); });
                    Assert.Throws<ArgumentException>(delegate           { blockStream.Write(buffer, 5, 10);  });
                }
            }
        }

        [Fact]
        public void EndOfFile()
        {
            using (var ms = new MemoryStream())
            {
                var writeBuffer = GetRandomBytes(100);
                var readBuffer  = new byte[60];

                using (var blockStream = new BlockStream(Zstd, ms, CompressionMode.Compress, true))
                {
                    blockStream.Write(writeBuffer, 0, writeBuffer.Length);
                }

                ms.Position = 0;

                using (var blockStream = new BlockStream(Zstd, ms, CompressionMode.Decompress))
                {
                    int numBytesRead = blockStream.Read(readBuffer, 0, readBuffer.Length);
                    Assert.Equal(readBuffer.Length, numBytesRead);

                    numBytesRead = blockStream.Read(readBuffer, 0, readBuffer.Length);
                    Assert.Equal(writeBuffer.Length - readBuffer.Length, numBytesRead);

                    numBytesRead = blockStream.Read(readBuffer, 0, readBuffer.Length);
                    Assert.Equal(0, numBytesRead);
                }
            }
        }

        [Fact]
        public void DoubleDispose()
        {
            using (var ms = new MemoryStream())
            {
                var blockStream = new BlockStream(Zstd, ms, CompressionMode.Compress);
                blockStream.Dispose();
                blockStream.Dispose();
            }
        }
    }

    public sealed class DemoHeader : Header
    {
        public readonly DemoCustomHeader Custom;

        public DemoHeader(string identifier, ushort schemaVersion, ushort dataVersion, Source source,
            long creationTimeTicks, GenomeAssembly genomeAssembly, DemoCustomHeader customHeader) : base(
            identifier, schemaVersion, dataVersion, source, creationTimeTicks, genomeAssembly)
        {
            Custom = customHeader;
        }

        public new void Write(BinaryWriter writer)
        {
            base.Write(writer);
            Custom.Write(writer);
        }

        public static DemoHeader Read(Stream stream)
        {
            DemoHeader header;

            using (var reader = new BinaryReader(stream, Encoding.Default, true))
            {
                var baseHeader   = Read(reader);
                var customHeader = DemoCustomHeader.Read(reader);

                header = new DemoHeader(baseHeader.Identifier, baseHeader.SchemaVersion, baseHeader.DataVersion,
                    baseHeader.Source, baseHeader.CreationTimeTicks, baseHeader.Assembly, customHeader);
            }

            return header;
        }
    }

    public sealed class DemoCustomHeader
    {
        public long FileOffset;
        public int InternalOffset;

        public DemoCustomHeader(long fileOffset, int internalOffset)
        {
            FileOffset     = fileOffset;
            InternalOffset = internalOffset;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(FileOffset);
            writer.Write(InternalOffset);
        }

        public static DemoCustomHeader Read(BinaryReader reader)
        {
            long fileOffset    = reader.ReadInt64();
            int internalOffset = reader.ReadInt32();
            return new DemoCustomHeader(fileOffset, internalOffset);
        }
    }
}
