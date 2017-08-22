using System;
using System.Runtime.InteropServices;
using Compression.Utilities;

namespace Compression.Algorithms
{
    public sealed class Zlib : ICompressionAlgorithm
    {
        private readonly int _compressionLevel;

        /// <summary>
        /// constructor
        /// </summary>
        public Zlib(int compressionLevel = 1)
        {
            _compressionLevel = compressionLevel;
            LibraryUtilities.CheckLibrary();
        }

        /// <summary>
        /// compresses a byte array and stores the result in the output byte array
        /// </summary>
        public int Compress(byte[] source, int srcLength, byte[] destination, int destLength)
        {
            if (destination == null || GetCompressedBufferBounds(srcLength) > destination.Length)
            {
                throw new InvalidOperationException("Zlib: Insufficient memory in destination buffer");
            }

            SafeNativeMethods.bgzf_compress(destination, ref destLength, source, srcLength, _compressionLevel);
            return destLength;
        }

        /// <summary>
        /// compresses a byte array and stores the result in the output byte array
        /// </summary>
        public int Decompress(byte[] source, int srcLength, byte[] destination, int destLength)
        {
            if (destination == null)
            {
                throw new InvalidOperationException("Zlib: Insufficient memory in destination buffer");
            }

            return SafeNativeMethods.bgzf_decompress(destination, destLength, source, srcLength);
        }

        /// <summary>
        /// returns the appropriate length of the decompression buffer
        /// </summary>
        public int GetDecompressedLength(byte[] source, int srcLength)
        {
            int pos = srcLength - 4;
            return source[pos + 3] << 24 | source[pos + 2] << 16 | source[pos + 1] << 8 | source[pos];
        }

        /// <summary>
        /// returns the appropriate length of the compression buffer
        /// </summary>
        public int GetCompressedBufferBounds(int srcLength)
        {
            // empirically derived via polynomial regression with additional padding added
            return (int)(srcLength * 1.06 + 32.5);
        }

        private static class SafeNativeMethods
        {
            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern int bgzf_decompress(byte[] uncompressedBlock, int uncompressedSize, byte[] compressedBlock, int compressedSize);

            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern int bgzf_compress(byte[] compressedBlock, ref int compressedLen, byte[] uncompressedBlock, int uncompressedLen, int compressionLevel);
        }
    }
}
