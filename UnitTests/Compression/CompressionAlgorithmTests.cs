using System;
using System.IO;
using System.Text;
using Compression.Algorithms;
using UnitTests.Compression.FileHandling;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.Compression
{
    public sealed class CompressionAlgorithmTests
    {
        private const int NumOriginalBytes = 20000;
        private readonly byte[] _originalBytes;
        private readonly byte[] _dictBuffer;

        public CompressionAlgorithmTests()
        {
            _originalBytes = BlockStreamTests.GetRandomBytes(NumOriginalBytes);
            _dictBuffer    = LoadZstandardDictionary();
        }

        private static byte[] LoadZstandardDictionary() => File.ReadAllBytes(Resources.TopPath("clinvar.dict"));

        [Fact]
        public void RoundTrip_Zstandard_WithDictionary()
        {
            const string expectedString = "\"id\":\"RCV000537631.1\",\"reviewStatus\":\"criteria provided, single submitter\",\"alleleOrigins\":[\"germline\"],\"refAllele\":\"G\",\"altAllele\":\"A\",\"phenotypes\":[\"Immunodeficiency 38 with basal ganglia calcification\"],\"medGenIds\":[\"C4015293\"],\"omimIds\":[\"616126\"],\"orphanetIds\":[\"319563\"],\"significance\":\"benign\",\"lastUpdatedDate\":\"2017-12-27\",\"pubMedIds\":[\"28492532\"]";
            byte[] expectedBytes        = Encoding.UTF8.GetBytes(expectedString);
            var zstd                    = new ZstandardDict(17, _dictBuffer);

            int compressedBufferSize    = zstd.GetCompressedBufferBounds(expectedBytes.Length);
            var observedCompressedBytes = new byte[compressedBufferSize];

            Assert.Throws<InvalidOperationException>(delegate
            {
                zstd.Compress(expectedBytes, expectedBytes.Length, null, compressedBufferSize);
            });

            int numCompressedBytes = zstd.Compress(expectedBytes, expectedBytes.Length, observedCompressedBytes,
                compressedBufferSize);

            int decompressedBufferSize    = expectedBytes.Length;
            var observedDecompressedBytes = new byte[decompressedBufferSize];

            Assert.Throws<InvalidOperationException>(delegate
            {
                zstd.Decompress(observedCompressedBytes, numCompressedBytes, null, decompressedBufferSize);
            });

            int numDecompressedBytes = zstd.Decompress(observedCompressedBytes, numCompressedBytes,
                observedDecompressedBytes, decompressedBufferSize);

            Assert.Equal(expectedBytes.Length, numDecompressedBytes);
            Assert.Equal(expectedBytes, observedDecompressedBytes);

            Assert.Throws<NotImplementedException>(delegate
            {
                zstd.GetDecompressedLength(observedCompressedBytes, numCompressedBytes);
            });
        }

        [Theory]
        [InlineData(CompressionAlgorithms.Zlib)]
        [InlineData(CompressionAlgorithms.Zstandard)]
        public void RoundTrip(CompressionAlgorithms ca)
        {
            var compressionAlgorithm = GetCompressionAlgorithm(ca);

            int compressedBufferSize    = compressionAlgorithm.GetCompressedBufferBounds(NumOriginalBytes);
            var observedCompressedBytes = new byte[compressedBufferSize];
            var smallBuffer             = new byte[10];

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
        Zlib,
        Zstandard
    }
}