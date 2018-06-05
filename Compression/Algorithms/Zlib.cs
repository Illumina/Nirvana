using System;
using System.Runtime.InteropServices;
using Compression.Utilities;

namespace Compression.Algorithms
{
    public sealed class Zlib : ICompressionAlgorithm
    {
        private readonly int _compressionLevel;

        public Zlib(int compressionLevel = 1)
        {
            _compressionLevel = compressionLevel;
            LibraryUtilities.CheckLibrary();
        }

        public int Compress(byte[] source, int srcLength, byte[] destination, int destLength)
        {
            if (destination == null || GetCompressedBufferBounds(srcLength) > destination.Length)
            {
                throw new InvalidOperationException("Zlib: Insufficient memory in destination buffer");
            }
            
            return SafeNativeMethods.bgzf_compress(destination, destLength, source, srcLength, _compressionLevel);
        }

        public int Decompress(byte[] source, int srcLength, byte[] destination, int destLength)
        {
            if (destination == null)
            {
                throw new InvalidOperationException("Zlib: Insufficient memory in destination buffer");
            }

            return SafeNativeMethods.bgzf_decompress(destination, destLength, source, srcLength);
        }

        public int GetDecompressedLength(byte[] source, int srcLength)
        {
            int pos = srcLength - 4;
            return source[pos + 3] << 24 | source[pos + 2] << 16 | source[pos + 1] << 8 | source[pos];
        }

        public int GetCompressedBufferBounds(int srcLength) => (int)(srcLength * 1.06 + 28);

        private static class SafeNativeMethods
        {
            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern int bgzf_decompress(byte[] uncompressedBlock, int uncompressedSize, byte[] compressedBlock, int compressedSize);

            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern int bgzf_compress(byte[] compressedBlock, int compressedLen, byte[] uncompressedBlock, int uncompressedLen, int compressionLevel);
        }
    }
}
