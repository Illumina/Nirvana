using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using Compression.Algorithms;
using Compression.DataStructures;
using Compression.FileHandling;
using ErrorHandling.Exceptions;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.Compression.FileHandling
{
    public sealed class BlockStreamTests : RandomFileBase
    {
        private const long NumTicks                         = 3;
        private const GenomeAssembly ExpectedGenomeAssembly = GenomeAssembly.hg19;
        private const string SmallString                    = "Testing 123";
        private const string FinalString                    = "Squeamish Ossifrage";

        private static readonly Random Random = new Random(10);
        private static readonly QuickLZ Qlz = new QuickLZ();

        [Fact]
        public void BlockStream_EndToEnd()
        {
            string expectedString = GetRandomString(Block.DefaultSize + 10000);

            var customHeader = new DemoCustomHeader(new BlockStream.BlockPosition());
            var header = new CacheHeader(CacheConstants.Identifier, CacheConstants.SchemaVersion,
                CacheConstants.DataVersion, Source.Ensembl, NumTicks, ExpectedGenomeAssembly, customHeader);

            using (var ms = new MemoryStream())
            {                
                WriteBlockStream(Qlz, header, customHeader, ms, expectedString);
                ms.Seek(0, SeekOrigin.Begin);
                ReadFromBlockStream(Qlz, ms, expectedString);
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
        private static void ReadFromBlockStream(ICompressionAlgorithm compressionAlgorithm, MemoryStream ms, string expectedRandomString)
        {
            using (var blockStream  = new BlockStream(compressionAlgorithm, ms, CompressionMode.Decompress))
            using (var reader = new ExtendedBinaryReader(blockStream))
            {
                CheckWriteException(blockStream);

                // grab the header
                var header = GetHeader(blockStream, out var customHeader);
                Assert.Equal(ExpectedGenomeAssembly, header.GenomeAssembly);

                // sequential string check
                CheckString(reader, expectedRandomString);
                CheckString(reader, SmallString);
                CheckString(reader, FinalString);

                // random access string check
                blockStream.SetBlockPosition(customHeader.DemoPosition);
                CheckString(reader, SmallString);
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private static void CheckString(IExtendedBinaryReader reader, string expectedString)
        {
            string s = reader.ReadAsciiString();           
            Assert.NotNull(s);
            Assert.Equal(expectedString.Length, s.Length);
            Assert.Equal(expectedString, s);
        }

        private static CacheHeader GetHeader(BlockStream blockStream, out DemoCustomHeader customHeader)
        {
            var header = (CacheHeader)blockStream.ReadHeader(CacheHeader.Read, DemoCustomHeader.Read);
            customHeader = header.CustomHeader as DemoCustomHeader;
            return header;
        }

        private static void WriteBlockStream(ICompressionAlgorithm compressionAlgorithm, CacheHeader header,
            DemoCustomHeader customHeader, MemoryStream ms, string s)
        {
            using (var blockStream  = new BlockStream(compressionAlgorithm, ms, CompressionMode.Compress, true))
            using (var writer = new ExtendedBinaryWriter(blockStream))
            {
                CheckReadException(blockStream);

                blockStream.WriteHeader(header.Write);

                var bp = new BlockStream.BlockPosition();

                // detect that we have written a block
                blockStream.GetBlockPosition(bp);
                writer.WriteOptAscii(s);
                blockStream.GetBlockPosition(bp);

                // here we write a test string that won't invoke a new block
                blockStream.GetBlockPosition(customHeader.DemoPosition);
                writer.WriteOptAscii(SmallString);
                blockStream.GetBlockPosition(bp);

                Assert.Equal(customHeader.DemoPosition.FileOffset, blockStream.Position);

                blockStream.Flush();

                // this will be flushed during dispose
                writer.WriteOptAscii(FinalString);
            }
        }

        private static void CheckReadException(BlockStream writer)
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

        private static void CheckWriteException(BlockStream reader)
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
                using (new BlockStream(Qlz, null, CompressionMode.Decompress))
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
                using (var writer = new BlockStream(Qlz, ms, CompressionMode.Compress, true))
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
            string randomPath = GetRandomPath();

            using (var writeStream = new FileStream(randomPath, FileMode.Create, FileAccess.Write))
            {
                Assert.Throws<ArgumentException>(delegate
                {
                    // ReSharper disable once AccessToDisposedClosure
                    using (new BlockStream(Qlz, writeStream, CompressionMode.Decompress))
                    {
                    }
                });
            }

            using (var readStream = FileUtilities.GetReadStream(randomPath))
            {
                Assert.Throws<ArgumentException>(delegate
                {
                    // ReSharper disable once AccessToDisposedClosure
                    using (new BlockStream(Qlz, readStream, CompressionMode.Compress))
                    {
                    }
                });
            }
        }

        [Fact]
        public void CanReadWriteSeek()
        {
            string randomPath = GetRandomPath();

            using (var writeStream = new FileStream(randomPath, FileMode.Create, FileAccess.Write))
            using (var blockStream = new BlockStream(Qlz, writeStream, CompressionMode.Compress))
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
                using (var blockStream = new BlockStream(Qlz, ms, CompressionMode.Compress))
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

                using (var blockStream = new BlockStream(Qlz, ms, CompressionMode.Compress, true))
                {
                    blockStream.Write(writeBuffer, 0, writeBuffer.Length);
                }

                ms.Position = 0;

                using (var blockStream = new BlockStream(Qlz, ms, CompressionMode.Decompress))
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
                var blockStream = new BlockStream(Qlz, ms, CompressionMode.Compress);
                blockStream.Dispose();
                blockStream.Dispose();
            }
        }
    }

    public sealed class DemoCustomHeader : ICustomCacheHeader
    {
        public readonly BlockStream.BlockPosition DemoPosition;

        public DemoCustomHeader(BlockStream.BlockPosition demoPosition)
        {
            DemoPosition = demoPosition;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(DemoPosition.FileOffset);
            writer.Write(DemoPosition.InternalOffset);
        }

        public static ICustomCacheHeader Read(BinaryReader reader)
        {
            var fileOffset     = reader.ReadInt64();
            var internalOffset = reader.ReadInt32();

            var bp = new BlockStream.BlockPosition
            {
                FileOffset     = fileOffset,
                InternalOffset = internalOffset
            };

            return new DemoCustomHeader(bp);
        }
    }
}
