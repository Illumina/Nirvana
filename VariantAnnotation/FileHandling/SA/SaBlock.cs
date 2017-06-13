using VariantAnnotation.FileHandling.Compression;

namespace VariantAnnotation.FileHandling.SA
{
    public class SaBlock
    {
        protected readonly ICompressionAlgorithm CompressionAlgorithm;
        protected readonly BlockHeader Header;

        protected readonly byte[] CompressedBlock;
        public readonly byte[] UncompressedBlock;

        private const int DefaultBufferSize = 524288;

        /// <summary>
        /// constructor
        /// </summary>
        protected SaBlock(ICompressionAlgorithm compressionAlgorithm, int size = DefaultBufferSize)
        {
            CompressionAlgorithm    = compressionAlgorithm;
            UncompressedBlock       = new byte[size];
            var compressedBlockSize = compressionAlgorithm.GetCompressedBufferBounds(size);
            CompressedBlock         = new byte[compressedBlockSize];
            Header                  = new BlockHeader();
        }
    }
}
