using System;
using System.Buffers;
using System.IO;
using Compression.Algorithms;

namespace Compression.Utilities
{
    public static class BlockExtensions
    {
        private static readonly Zstandard Zstd = new(21);

        public static byte[] ReadCompressedByteArray(this BinaryReader reader, ArrayPool<byte> bytePool)
        {
            int uncompressedSize = reader.ReadInt32();
            int compressedSize   = reader.ReadInt32();

            byte[] compressedBuffer   = bytePool.Rent(compressedSize);
            byte[] uncompressedBuffer = bytePool.Rent(uncompressedSize);
            reader.Read(compressedBuffer, 0, compressedSize);

            Zstd.Decompress(compressedBuffer, compressedSize, uncompressedBuffer, uncompressedBuffer.Length);

            bytePool.Return(compressedBuffer);
            return uncompressedBuffer;
        }

        public static void WriteCompressedByteArray(this BinaryWriter writer, byte[] uncompressed, int uncompressedSize)
        {
            ArrayPool<byte> bytePool             = ArrayPool<byte>.Shared;
            int             compressedBufferSize = Zstd.GetCompressedBufferBounds(uncompressedSize);
            byte[]          compressedBuffer     = bytePool.Rent(compressedBufferSize);

            int compressedSize = Zstd.Compress(uncompressed, uncompressedSize, compressedBuffer, compressedBuffer.Length);

            writer.Write(uncompressedSize);
            writer.Write(compressedSize);
            writer.Write(compressedBuffer, 0, compressedSize);

            double percentCompression = compressedSize / (double) uncompressedSize * 100.0;
            Console.WriteLine($"uncompressed: {uncompressedSize:N0}, compressed: {compressedSize:N0}, {percentCompression:0.0}%");

            bytePool.Return(compressedBuffer);
        }
    }
}