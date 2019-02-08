using System;
using Compression.Algorithms;

namespace VariantAnnotation.NSA
{
    public static class NsaUtilities
    {
        public static int GetCompressedBytes(ICompressionAlgorithm compressionAlgorithm, byte[] uncompressedBytes, int length, byte[] compressedBuffer)
        {
            int estimatedCompressionSize = compressionAlgorithm.GetCompressedBufferBounds(length);

            if (estimatedCompressionSize > ushort.MaxValue)
                throw new OverflowException("annotation record is unexpectedly large!! " + estimatedCompressionSize);

            return compressionAlgorithm.Compress(uncompressedBytes, length, compressedBuffer, estimatedCompressionSize);
        }
    }
}