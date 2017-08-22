using System;
using Compression.Algorithms;
using UnitTests.Compression.FileHandling;
using Xunit;

namespace UnitTests.Compression
{
    public sealed class CompressionAlgorithmTests
    {
        private const int NumOriginalBytes = 20000;
        private readonly byte[] _originalBytes;

        /// <summary>
        /// constructor
        /// </summary>
        public CompressionAlgorithmTests()
        {
            _originalBytes = BlockStreamTests.GetRandomBytes(NumOriginalBytes);
        }

        [Theory]
        [InlineData(CompressionAlgorithms.QuickLz)]
        [InlineData(CompressionAlgorithms.Zlib)]
        [InlineData(CompressionAlgorithms.Zstandard)]
        public void RoundTrip(CompressionAlgorithms ca)
        {
            var compressionAlgorithm = GetCompressionAlgorithm(ca);

            int compressedBufferSize = compressionAlgorithm.GetCompressedBufferBounds(NumOriginalBytes);
            var observedCompressedBytes = new byte[compressedBufferSize];
            var smallBuffer = new byte[10];

            Assert.Throws<InvalidOperationException>(delegate
            {
                compressionAlgorithm.Compress(_originalBytes, NumOriginalBytes, null, compressedBufferSize);
            });

            Assert.Throws<InvalidOperationException>(delegate
            {
                compressionAlgorithm.Compress(_originalBytes, NumOriginalBytes, smallBuffer, compressedBufferSize);
            });

            int numCompressedBytes = compressionAlgorithm.Compress(_originalBytes, NumOriginalBytes, observedCompressedBytes,
                compressedBufferSize);

            int decompressedBufferSize = compressionAlgorithm.GetDecompressedLength(observedCompressedBytes, numCompressedBytes);
            var observedDecompressedBytes = new byte[decompressedBufferSize];

            Assert.Throws<InvalidOperationException>(delegate
            {
                compressionAlgorithm.Decompress(observedCompressedBytes, numCompressedBytes, null, decompressedBufferSize);
            });

            int numDecompressedBytes = compressionAlgorithm.Decompress(observedCompressedBytes, numCompressedBytes,
                observedDecompressedBytes, decompressedBufferSize);

            Assert.Equal(NumOriginalBytes, numDecompressedBytes);
            Assert.Equal(_originalBytes, observedDecompressedBytes);
        }

        private static ICompressionAlgorithm GetCompressionAlgorithm(CompressionAlgorithms ca)
        {
            switch (ca)
            {
                case CompressionAlgorithms.QuickLz:
                    return new QuickLZ();
                case CompressionAlgorithms.Zlib:
                    return new Zlib();
                case CompressionAlgorithms.Zstandard:
                    return new Zstandard();
                default:
                    throw new InvalidOperationException($"Unknown compression algorithm: {ca}");
            }
        }
    }

    public enum CompressionAlgorithms
    {
        QuickLz,
        Zlib,
        Zstandard
    }
}