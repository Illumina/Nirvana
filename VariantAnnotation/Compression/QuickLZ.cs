using System;
using System.Runtime.InteropServices;
using ErrorHandling.Exceptions;

namespace VariantAnnotation.Compression
{
    public class QuickLZ
    {
        #region members

        public const int CompressionOverhead = 400;
        
        #endregion

        // constructor
        public QuickLZ()
        {
            string compressionLibraryName = Type.GetType("Mono.Runtime") == null
                ? "BlockCompression.dll"
                : "libBlockCompression.so";

            // check to see if we have our compression library
            try
            {
                Marshal.PtrToStringAnsi(SafeNativeMethods.GetVersion());
            }
            catch (Exception)
            {
                throw new MissingCompressionLibraryException(compressionLibraryName);
            }
        }

        /// <summary>
        /// compresses a source byte array and stores the compressed bytes in the destination byte array
        /// </summary>
        public int Compress(byte[] source, ref byte[] destination)
        {
            // make sure the destination is large enough
            if ((destination == null) || (source.Length + CompressionOverhead > destination.Length))
            {
                destination = new byte[source.Length + CompressionOverhead];
            }

            return SafeNativeMethods.QuickLzCompress(source, source.Length, destination, destination.Length);
        }

		public int Compress(byte[] source, int srcLength, byte[] destination, int destLength)
		{
			if ((destination == null) || (srcLength + CompressionOverhead > destLength))
			{
				throw new InsufficientMemoryException("QuickLZ: Insufficient memeory in destination buffer");
			}

			return SafeNativeMethods.QuickLzCompress(source, srcLength, destination, destLength);
		}

		/// <summary>
		/// decompresses a source byte array and stores the uncompressed bytes in the destination byte array
		/// </summary>
		public int Decompress(byte[] source, ref byte[] destination)
        {
            // make sure the destination is large enough
            int requiredBufferSize = (int)SafeNativeMethods.qlz_size_decompressed(source);

            if ((destination == null) || (requiredBufferSize > destination.Length))
            {
                destination = new byte[requiredBufferSize];
            }

            return SafeNativeMethods.QuickLzDecompress(source, destination, destination.Length);
        }

        private static class SafeNativeMethods
        {
            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern IntPtr GetVersion();

            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern long qlz_size_decompressed(byte[] source);

            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern int QuickLzCompress(byte[] source, int sourceLen, byte[] destination, int destinationLen);

            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern int QuickLzDecompress(byte[] source, byte[] destination, int destinationLen);
        }
	}
}
