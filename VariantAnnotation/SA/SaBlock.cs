using Compression.Algorithms;
using Compression.FileHandling;

namespace VariantAnnotation.SA
{
    public class SaBlock
    {
        protected readonly ICompressionAlgorithm CompressionAlgorithm;
        protected readonly BlockHeader Header;

        protected readonly byte[] CompressedBlock;
        public readonly byte[] UncompressedBlock;

        /// <summary>
        /// constructor
        /// </summary>
        protected SaBlock(ICompressionAlgorithm compressionAlgorithm, int size)
        {
            CompressionAlgorithm = compressionAlgorithm;
            UncompressedBlock = new byte[size];
            var compressedBlockSize = compressionAlgorithm.GetCompressedBufferBounds(size);
            CompressedBlock = new byte[compressedBlockSize];
            Header = new BlockHeader();
        }
    }
}