using System;
using System.IO;
using ErrorHandling.Exceptions;

namespace VariantAnnotation.Compression
{
    public class QuickLzBlock
    {
        #region members

        private readonly QuickLZ _qlz;
        private readonly QuickLzBlockHeader _header;

        private readonly byte[] _compressedBlock;
        private byte[] _uncompressedBlock;
        public int Offset { get; private set; }

        private const int Size = 16777216;
        private readonly int _compressedBlockSize;

        public bool IsFull => Offset == Size;
        public bool HasMoreData => Offset < _header.NumUncompressedBytes;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public QuickLzBlock()
        {
            Offset = 0;
            _compressedBlockSize = Size + QuickLZ.CompressionOverhead;

            _compressedBlock = new byte[Size];
            _uncompressedBlock = new byte[Size];

            _qlz = new QuickLZ();
            _header = new QuickLzBlockHeader();
        }

        /// <summary>
        /// Copies bytes from the specified byte array to the underlying uncompressed buffer.
        /// </summary>
        /// <param name="array">The buffer that contains the data.</param>
        /// <param name="offset">The byte offset in <paramref name="array"/> from which the bytes will be read.</param>
        /// <param name="count">The maximum number of bytes to copy.</param>
        public int CopyTo(byte[] array, int offset, int count)
        {
            int copyLength = Math.Min(Size - Offset, count);
            if (copyLength == 0) return 0;

            Buffer.BlockCopy(array, offset, _uncompressedBlock, Offset, copyLength);
            Offset += copyLength;

            return copyLength;
        }

        /// <summary>
        /// Copies bytes from the underlying uncompressed buffer to the specified byte array. 
        /// </summary>
        /// <param name="array">The buffer that contains the data.</param>
        /// <param name="offset">The byte offset in <paramref name="array"/> from which the bytes will be read.</param>
        /// <param name="count">The maximum number of bytes to copy.</param>
        public int CopyFrom(byte[] array, int offset, int count)
        {
            int copyLength = Math.Min(_header.NumUncompressedBytes - Offset, count);
            if (copyLength == 0) return 0;

            Buffer.BlockCopy(_uncompressedBlock, Offset, array, offset, copyLength);
            Offset += copyLength;

            return copyLength;
        }

        /// <summary>
        /// Writes the current QuickLZ block to a stream.
        /// </summary>
        /// <param name="stream">The stream that will write the compressed data.</param>
        public void Write(Stream stream)
        {
            _header.NumUncompressedBytes = Offset;
            _header.NumCompressedBytes = _qlz.Compress(_uncompressedBlock, _header.NumUncompressedBytes, _compressedBlock, _compressedBlockSize);

            _header.Write(stream);
            stream.Write(_compressedBlock, 0, _header.NumCompressedBytes);

            Offset = 0;
        }

        /// <summary>
        /// Writes the EOF header to a stream.
        /// </summary>
        /// <param name="stream">The stream that will write the compressed data.</param>
        public void WriteEof(Stream stream)
        {
            _header.NumUncompressedBytes = -1;
            _header.NumCompressedBytes = -1;
            _header.Write(stream);
        }

        /// <summary>
        /// Reads the next QuickLZ block from the stream.
        /// </summary>
        /// <param name="stream">The stream that will read the compressed data.</param>
        public int Read(Stream stream)
        {
            _header.Read(stream);
            if (_header.IsEmpty) return -1;

            int numBytesRead = stream.Read(_compressedBlock, 0, _header.NumCompressedBytes);
            if (numBytesRead != _header.NumCompressedBytes) throw new IOException($"Expected {_header.NumCompressedBytes} bytes from the block, but received only {numBytesRead} bytes.");

            int numUncompressedBytes = _qlz.Decompress(_compressedBlock, ref _uncompressedBlock);
            if (numUncompressedBytes != _header.NumUncompressedBytes) throw new CompressionException($"Expected {_header.NumUncompressedBytes} bytes after decompression, but found only {numUncompressedBytes} bytes.");

            Offset = 0;

            return QuickLzBlockHeader.HeaderSize + numBytesRead;
        }
    }
}
