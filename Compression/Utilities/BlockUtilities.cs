using System.Buffers;
using System.IO;
using Compression.Algorithms;

namespace Compression.Utilities;

public static class BlockUtilities
{
    private static readonly Zstandard Zstd = new(22);

    public static byte[] ReadCompressedByteArray(this BinaryReader reader, ArrayPool<byte> bytePool)
    {
        int uncompressedSize = reader.ReadInt32();
        int compressedSize   = reader.ReadInt32();

        byte[] compressedBuffer   = bytePool.Rent(compressedSize);
        byte[] uncompressedBuffer = bytePool.Rent(uncompressedSize);
        int    numRead            = reader.Read(compressedBuffer, 0, compressedSize);

        int len = Zstd.Decompress(compressedBuffer, compressedSize, uncompressedBuffer, uncompressedBuffer.Length);

        bytePool.Return(compressedBuffer);
        return uncompressedBuffer;
    }

    public static (int CompressedSize, double PercentCompression) WriteCompressedByteArray(this BinaryWriter writer,
        byte[] uncompressed, int uncompressedSize)
    {
        ArrayPool<byte> bytePool             = ArrayPool<byte>.Shared;
        int             compressedBufferSize = Zstd.GetCompressedBufferBounds(uncompressedSize);
        byte[]          compressedBuffer     = bytePool.Rent(compressedBufferSize);

        int compressedSize = Zstd.Compress(uncompressed, uncompressedSize, compressedBuffer, compressedBuffer.Length);

        writer.Write(uncompressedSize);
        writer.Write(compressedSize);
        writer.Write(compressedBuffer, 0, compressedSize);

        double percentCompression = compressedSize / (double) uncompressedSize;
        bytePool.Return(compressedBuffer);

        return (compressedSize, percentCompression);
    }
}