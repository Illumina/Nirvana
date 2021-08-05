using System;
using System.Runtime.InteropServices;
using Compression.Utilities;

namespace Compression.Algorithms
{
    public sealed class QuickLZ : ICompressionAlgorithm
    {
        private const int CompressionOverhead = 400;

        public QuickLZ() => LibraryUtilities.CheckLibrary();

        public int Compress(byte[] source, int srcLength, byte[] destination, int destLength)
		{
			if (destination == null || GetCompressedBufferBounds(srcLength) > destination.Length)
            {
				throw new InvalidOperationException("QuickLZ: Insufficient memory in destination buffer");
			}

			return SafeNativeMethods.QuickLzCompress(source, srcLength, destination, destLength);
		}

        public int Decompress(byte[] source, int srcLength, byte[] destination, int destLength)
        {
            if (destination == null)
            {
                throw new InvalidOperationException("QuickLZ: Insufficient memory in destination buffer");
            }

            return SafeNativeMethods.QuickLzDecompress(source, destination, destLength);
        }

        public int GetDecompressedLength(byte[] source, int srcLength) => (int)SafeNativeMethods.qlz_size_decompressed(source);

        public int GetCompressedBufferBounds(int srcLength) => srcLength + CompressionOverhead;

        private static class SafeNativeMethods
        {
            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern long qlz_size_decompressed(byte[] bytes);

            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern int QuickLzCompress(byte[] source, int sourceLen, byte[] destination, int destinationLen);

            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern int QuickLzDecompress(byte[] source, byte[] destination, int destinationLen);
        }
	}
}
