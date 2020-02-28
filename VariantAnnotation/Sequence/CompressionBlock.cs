using System.IO;
using Compression.Algorithms;
using ErrorHandling.Exceptions;

namespace VariantAnnotation.Sequence
{
    public sealed class CompressionBlock
    {
        private byte[] _uncompressedBuffer;
        private byte[] _compressedBuffer;

        private readonly int _compressedBufferSize;

        public byte[] Buffer { get; private set; }
        public int BufferSize { get; private set; }

        public readonly ushort RefIndex;

        public int UncompressedBufferSize { get; }
        public int CompressedBufferSize { get; private set; }

        private static readonly Zstandard Zstd = new Zstandard(21);

        public CompressionBlock(ushort refIndex, byte[] uncompressedBuffer, int numBytes)
        {
            RefIndex               = refIndex;
            _uncompressedBuffer    = uncompressedBuffer;
            UncompressedBufferSize = numBytes;
            _compressedBufferSize  = Zstd.GetCompressedBufferBounds(numBytes);
            _compressedBuffer      = new byte[_compressedBufferSize];
        }

        public void Compress()
        {
            CompressedBufferSize = Zstd.Compress(_uncompressedBuffer, UncompressedBufferSize, _compressedBuffer,
                _compressedBufferSize);

            if (CompressedBufferSize > UncompressedBufferSize)
            {
                _compressedBuffer    = null;
                CompressedBufferSize = -1;

                Buffer     = _uncompressedBuffer;
                BufferSize = UncompressedBufferSize;
            }
            else
            {
                _uncompressedBuffer = null;

                Buffer     = _compressedBuffer;
                BufferSize = CompressedBufferSize;
            }
        }

        public static byte[] Read(Stream stream, int uncompressedBufferSize, int compressedBufferSize)
        {
            return compressedBufferSize == -1
                ? ReadUncompressedBlock(stream, uncompressedBufferSize)
                : ReadCompressedBlock(stream, uncompressedBufferSize, compressedBufferSize);
        }

        private static byte[] ReadCompressedBlock(Stream stream, int uncompressedBufferSize, int compressedBufferSize)
        {
            var compressedBuffer = new byte[compressedBufferSize];
            var buffer           = new byte[uncompressedBufferSize];

            int numBytesRead = stream.Read(compressedBuffer, 0, compressedBufferSize);
            if (numBytesRead != compressedBufferSize)
            {
                throw new IOException($"Expected {compressedBufferSize} bytes from the block, but received only {numBytesRead} bytes.");
            }

            int numUncompressedBytes = Zstd.Decompress(compressedBuffer, compressedBufferSize, buffer, uncompressedBufferSize);
            if (numUncompressedBytes != uncompressedBufferSize)
            {
                throw new CompressionException($"Expected {uncompressedBufferSize} bytes after decompression, but found only {numUncompressedBytes} bytes.");
            }

            return buffer;
        }

        private static byte[] ReadUncompressedBlock(Stream stream, int uncompressedBufferSize)
        {
            var buffer = new byte[uncompressedBufferSize];
            int numBytesRead = stream.Read(buffer, 0, uncompressedBufferSize);

            if (numBytesRead != uncompressedBufferSize)
            {
                throw new IOException($"Expected {uncompressedBufferSize} bytes from the uncompressed block, but received only {numBytesRead} bytes.");
            }

            return buffer;
        }
    }
}
