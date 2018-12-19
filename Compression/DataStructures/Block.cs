using System;
using System.IO;
using Compression.Algorithms;
using Compression.FileHandling;
using ErrorHandling.Exceptions;

namespace Compression.DataStructures
{
    public sealed class Block
    {
        private readonly ICompressionAlgorithm _compressionAlgorithm;
        private readonly BlockHeader _header;

        private readonly byte[] _compressedBlock;
        private readonly byte[] _uncompressedBlock;

        public long FileOffset { get; private set; }
        public int Offset { get; internal set; }

        internal const int DefaultSize = 16777216;
        private readonly int _size;
        private readonly int _compressedBlockSize;

        public bool IsFull      => Offset == _size;
        public bool HasMoreData => Offset < _header.NumUncompressedBytes;

        public Block(ICompressionAlgorithm compressionAlgorithm, int size = DefaultSize)
        {
            _compressionAlgorithm = compressionAlgorithm;
            Offset                = 0;
            _size                 = size;

            _uncompressedBlock   = new byte[_size];
            _compressedBlockSize = compressionAlgorithm.GetCompressedBufferBounds(_size);
            _compressedBlock     = new byte[_compressedBlockSize];
            _header              = new BlockHeader();
        }

        public int CopyTo(byte[] array, int offset, int count)
        {
            int copyLength = Math.Min(_size - Offset, count);
            if (copyLength == 0) return 0;

            Buffer.BlockCopy(array, offset, _uncompressedBlock, Offset, copyLength);
            Offset += copyLength;

            return copyLength;
        }

        public int CopyFrom(byte[] array, int offset, int count)
        {
            int copyLength = Math.Min(_header.NumUncompressedBytes - Offset, count);
            if (copyLength == 0) return 0;

            Buffer.BlockCopy(_uncompressedBlock, Offset, array, offset, copyLength);
            Offset += copyLength;

            return copyLength;
        }

        public void Write(Stream stream)
        {
            _header.NumUncompressedBytes = Offset;

            _header.NumCompressedBytes = _compressionAlgorithm.Compress(_uncompressedBlock, _header.NumUncompressedBytes,
                _compressedBlock, _compressedBlockSize);

            if (_header.NumCompressedBytes > _header.NumUncompressedBytes)
            {
                _header.NumCompressedBytes = -1;
                _header.Write(stream);
                stream.Write(_uncompressedBlock, 0, _header.NumUncompressedBytes);
            }
            else
            {
                _header.Write(stream);
                stream.Write(_compressedBlock, 0, _header.NumCompressedBytes);
            }

            Offset = 0;
        }

        public void WriteEof(Stream stream)
        {
            _header.NumUncompressedBytes = -1;
            _header.NumCompressedBytes   = -1;
            _header.Write(stream);
        }

        public int Read(Stream stream)
        {
            FileOffset = stream.Position;

            _header.Read(stream);
            if (_header.IsEmpty) return -1;

            int numBytesRead = _header.NumCompressedBytes == -1
                ? ReadUncompressedBlock(stream)
                : ReadCompressedBlock(stream);

            Offset = 0;

            return BlockHeader.HeaderSize + numBytesRead;
        }

        private int ReadCompressedBlock(Stream stream)
        {
            int numBytesRead = stream.Read(_compressedBlock, 0, _header.NumCompressedBytes);
            if (numBytesRead != _header.NumCompressedBytes)
            {
                throw new IOException($"Expected {_header.NumCompressedBytes} bytes from the block, but received only {numBytesRead} bytes.");
            }

            int numUncompressedBytes = _compressionAlgorithm.Decompress(_compressedBlock, _header.NumCompressedBytes, _uncompressedBlock, _size);
            if (numUncompressedBytes != _header.NumUncompressedBytes)
            {
                throw new CompressionException($"Expected {_header.NumUncompressedBytes} bytes after decompression, but found only {numUncompressedBytes} bytes.");
            }

            return numBytesRead;
        }

        private int ReadUncompressedBlock(Stream stream)
        {
            int numBytesRead = stream.Read(_uncompressedBlock, 0, _header.NumUncompressedBytes);
            if (numBytesRead != _header.NumUncompressedBytes)
            {
                throw new IOException($"Expected {_header.NumUncompressedBytes} bytes from the uncompressed block, but received only {numBytesRead} bytes.");
            }

            return numBytesRead;
        }
    }
}
