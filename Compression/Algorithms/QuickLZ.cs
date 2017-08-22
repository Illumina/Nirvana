using System;
using System.Runtime.InteropServices;
using Compression.Utilities;

namespace Compression.Algorithms
{
    public sealed class QuickLZ : ICompressionAlgorithm
    {
        #region members

        private const int CompressionOverhead = 400;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public QuickLZ()
        {
            LibraryUtilities.CheckLibrary();
        }

        /// <summary>
        /// compresses a source byte array and stores the compressed bytes in the destination byte array
        /// </summary>
        public int Compress(byte[] source, int srcLength, byte[] destination, int destLength)
		{
			if (destination == null || GetCompressedBufferBounds(srcLength) > destination.Length)
            {
				throw new InvalidOperationException("QuickLZ: Insufficient memory in destination buffer");
			}

			return SafeNativeMethods.QuickLzCompress(source, srcLength, destination, destLength);
		}

        /// <summary>
        /// decompresses a source byte array and stores the uncompressed bytes in the destination byte array
        /// </summary>
        public int Decompress(byte[] source, int srcLength, byte[] destination, int destLength)
        {
            if (destination == null)
            {
                throw new InvalidOperationException("QuickLZ: Insufficient memory in destination buffer");
            }

            return SafeNativeMethods.QuickLzDecompress(source, destination, destLength);
        }

        /// <summary>
        /// returns the appropriate length of the decompression buffer
        /// </summary>
        public int GetDecompressedLength(byte[] source, int srcLength)
        {
            return (int)SafeNativeMethods.qlz_size_decompressed(source);
        }

        /// <summary>
        /// returns the appropriate length of the compression buffer
        /// </summary>
        public int GetCompressedBufferBounds(int srcLength)
        {
            return srcLength + CompressionOverhead;
        }

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
