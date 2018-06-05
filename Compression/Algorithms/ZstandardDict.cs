using System;
using System.Runtime.InteropServices;
using Compression.Utilities;

namespace Compression.Algorithms
{
    public sealed class ZstandardDict : ICompressionAlgorithm
    {
        private readonly IntPtr _compressDict;
        private readonly IntPtr _decompressDict;
        private readonly IntPtr _compressContext;
        private readonly IntPtr _decompressContext;

        public ZstandardDict(int compressionLevel, byte[] dictBuffer)
        {
            LibraryUtilities.CheckLibrary();

            _compressDict      = SafeNativeMethods.ZSTD_createCDict(dictBuffer, (ulong)dictBuffer.Length, compressionLevel);
            _decompressDict    = SafeNativeMethods.ZSTD_createDDict(dictBuffer, (ulong)dictBuffer.Length);
            _compressContext   = SafeNativeMethods.ZSTD_createCCtx();
            _decompressContext = SafeNativeMethods.ZSTD_createDCtx();
        }

        public int Compress(byte[] source, int srcLength, byte[] destination, int destLength)
        {
            if (destination == null) throw new InvalidOperationException("Zstandard: Insufficient memory in destination buffer");
            return (int)SafeNativeMethods.ZSTD_compress_usingCDict(_compressContext, destination, (ulong)destLength, source, (ulong)srcLength, _compressDict);
        }

        public int Decompress(byte[] source, int srcLength, byte[] destination, int destLength)
        {
            if (destination == null) throw new InvalidOperationException("Zstandard: Insufficient memory in destination buffer");
            return (int)SafeNativeMethods.ZSTD_decompress_usingDDict(_decompressContext, destination, (ulong)destLength, source, (ulong)srcLength, _decompressDict);
        }

        public int GetDecompressedLength(byte[] source, int srcLength) => throw new NotImplementedException();

        // empirically derived via polynomial regression with additional padding added
        public int GetCompressedBufferBounds(int srcLength) => srcLength + 32;

        private static class SafeNativeMethods
        {
            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern ulong ZSTD_compress_usingCDict(IntPtr cctx, byte[] destination, ulong destinationLen, byte[] source, ulong sourceLen, IntPtr cdict);

            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern ulong ZSTD_decompress_usingDDict(IntPtr dctx, byte[] destination, ulong destinationLen, byte[] source, ulong sourceLen, IntPtr ddict);

            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr ZSTD_createCDict(byte[] dictBuffer, ulong dictSize, int compressionLevel);

            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr ZSTD_createDDict(byte[] dictBuffer, ulong dictSize);

            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr ZSTD_createCCtx();

            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr ZSTD_createDCtx();
        }
    }
}
