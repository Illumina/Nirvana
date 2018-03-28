using System.IO;
using ErrorHandling.Exceptions;

namespace Compression.FileHandling
{
    public sealed class BlockHeader
    {
        private readonly byte[] _header;

        public const int HeaderSize = 12;
        private const int HeaderId = -822411574; // cafeface

        public int NumUncompressedBytes;
        public int NumCompressedBytes;

        public bool IsEmpty => NumUncompressedBytes == -1 && NumCompressedBytes == -1;

        /// <summary>
        /// constructor
        /// </summary>
        public BlockHeader()
        {
            _header = new byte[HeaderSize];
        }

        /// <summary>
        /// Returns the integer that is encoded in the header at the specified offset.
        /// </summary>
        /// <param name="offset">The byte offset in the header.</param>
        /// <returns>Stored integer.</returns>
        private int GetInt(int offset)
        {
            return _header[offset] | _header[offset + 1] << 8 | _header[offset + 2] << 16 | _header[offset + 3] << 24;
        }

        /// <summary>
        /// Reads the header from the supplied stream
        /// </summary>
        /// <param name="stream">The input stream.</param>
        public void Read(Stream stream)
        {
            int numBytesRead = stream.Read(_header, 0, HeaderSize);
            if (numBytesRead != HeaderSize) throw new IOException($"Expected {HeaderSize} bytes from the block header, but received only {numBytesRead} bytes.");

            int headerId = GetInt(0);
            if (headerId != HeaderId) throw new CompressionException($"Expected the header ID ({HeaderId}), but found the following: {headerId}");

            NumUncompressedBytes = GetInt(4);
            NumCompressedBytes = GetInt(8);
        }

        /// <summary>
        /// Writes the specified integer into the header
        /// </summary>
        /// <param name="value">The integer that will be written into the header</param>
        /// <param name="offset">The byte offset in the header.</param>
        private void SetInt(int value, int offset)
        {
            _header[offset]     = (byte)value;
            _header[offset + 1] = (byte)(value >> 8);
            _header[offset + 2] = (byte)(value >> 16);
            _header[offset + 3] = (byte)(value >> 24);
        }

        /// <summary>
        /// Writes the header to the supplied stream
        /// </summary>
        /// <param name="stream">The output stream.</param>
        public void Write(Stream stream)
        {
            SetInt(HeaderId, 0);
            SetInt(NumUncompressedBytes, 4);
            SetInt(NumCompressedBytes, 8);
            stream.Write(_header, 0, HeaderSize);
        }
    }
}
