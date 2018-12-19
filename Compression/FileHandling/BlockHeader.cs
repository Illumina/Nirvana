using System.IO;
using ErrorHandling.Exceptions;

namespace Compression.FileHandling
{
    public sealed class BlockHeader
    {
        private readonly byte[] _header;

        public const int HeaderSize = 12;
        private const int HeaderId  = -822411574; // cafeface

        public int NumUncompressedBytes;
        public int NumCompressedBytes;

        public bool IsEmpty => NumUncompressedBytes == -1 && NumCompressedBytes == -1;

        public BlockHeader() => _header = new byte[HeaderSize];

        private int GetInt(int offset) => _header[offset] | _header[offset + 1] << 8 | _header[offset + 2] << 16 |
                                          _header[offset + 3] << 24;

        public void Read(Stream stream)
        {
            int numBytesRead = stream.Read(_header, 0, HeaderSize);

            if (numBytesRead == 0)
            {
                NumUncompressedBytes = -1;
                NumCompressedBytes   = -1;
                return;
            }

            if (numBytesRead != HeaderSize) throw new IOException($"Expected {HeaderSize} bytes from the block header, but received only {numBytesRead} bytes.");

            int headerId = GetInt(0);
            if (headerId != HeaderId) throw new CompressionException($"Expected the header ID ({HeaderId}), but found the following: {headerId}");

            NumUncompressedBytes = GetInt(4);
            NumCompressedBytes   = GetInt(8);
        }

        private void SetInt(int value, int offset)
        {
            _header[offset]     = (byte)value;
            _header[offset + 1] = (byte)(value >> 8);
            _header[offset + 2] = (byte)(value >> 16);
            _header[offset + 3] = (byte)(value >> 24);
        }

        public void Write(Stream stream)
        {
            SetInt(HeaderId, 0);
            SetInt(NumUncompressedBytes, 4);
            SetInt(NumCompressedBytes, 8);
            stream.Write(_header, 0, HeaderSize);
        }
    }
}
