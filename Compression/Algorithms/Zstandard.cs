using System;
using System.Runtime.InteropServices;
using Compression.Utilities;

namespace Compression.Algorithms
{
    public sealed class Zstandard : ICompressionAlgorithm
    {
        private readonly int _compressionLevel;

        public Zstandard(int compressionLevel = 17)
        {
            _compressionLevel = compressionLevel;
            LibraryUtilities.CheckLibrary();
        }

        public int Compress(byte[] source, int srcLength, byte[] destination, int destLength)
        {
            if (destination == null || GetCompressedBufferBounds(srcLength) > destination.Length)
            {
                throw new InvalidOperationException("Zstandard: Insufficient memory in destination buffer");
            }

            return (int)SafeNativeMethods.ZSTD_compress(destination, (ulong)destLength, source, (ulong)srcLength, _compressionLevel);
        }

        public int Decompress(byte[] source, int srcLength, byte[] destination, int destLength)
        {
            if (destination == null)
            {
                throw new InvalidOperationException("Zstandard: Insufficient memory in destination buffer");
            }

            return (int)SafeNativeMethods.ZSTD_decompress(destination, (ulong)destLength, source, (ulong)srcLength);
        }

        public int GetDecompressedLength(byte[] source, int srcLength) => (int)SafeNativeMethods.ZSTD_getDecompressedSize(source, srcLength);

        // empirically derived via polynomial regression with additional padding added
        public int GetCompressedBufferBounds(int srcLength) => srcLength + 32;

        private static class SafeNativeMethods
        {
            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern ulong ZSTD_compress(byte[] destination, ulong destinationLen, byte[] source, ulong sourceLen, int compressionLevel);

            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern ulong ZSTD_decompress(byte[] destination, ulong destinationLen, byte[] source, ulong sourceLen);

            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern ulong ZSTD_getDecompressedSize(byte[] source, int sourceLen);
        }
    }
}
