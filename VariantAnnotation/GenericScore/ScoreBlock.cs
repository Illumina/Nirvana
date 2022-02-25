using System;
using Compression.Algorithms;
using IO;

namespace VariantAnnotation.GenericScore
{
    public sealed class ScoreBlock
    {
        private readonly ICompressionAlgorithm _compressionAlgorithm;

        private readonly byte[] _compressedBytes;
        private readonly byte[] _uncompressedBytes;
        private          uint   _cursorPosition;
        private readonly int    _blockSize;


        public ScoreBlock(ICompressionAlgorithm compressionAlgorithm, int blockSize)
        {
            _compressionAlgorithm = compressionAlgorithm;
            _blockSize            = blockSize;

            int compressedBlockSize = _compressionAlgorithm.GetCompressedBufferBounds(_blockSize);

            _compressedBytes   = new byte[compressedBlockSize];
            _uncompressedBytes = new byte[_blockSize];
            Clear();
        }

        private void Clear()
        {
            Array.Fill(_uncompressedBytes, byte.MaxValue);
            _cursorPosition = 0;
        }

        public bool IsFull()
        {
            return _cursorPosition == _blockSize;
        }

        public void Add(uint memoryIndex, byte[] variableArray, uint arraySize)
        {
            Array.Copy(variableArray, 0, _uncompressedBytes, memoryIndex, arraySize);
            _cursorPosition = (memoryIndex + arraySize);
        }

        public (uint uncompressedSize, int compressedSize) Write(ExtendedBinaryWriter writer)
        {
            int compressedSize = _compressionAlgorithm.Compress(
                _uncompressedBytes,
                _blockSize,
                _compressedBytes,
                _compressedBytes.Length
            );

            writer.Write(_compressedBytes, 0, compressedSize);
            uint uncompressedSize = _cursorPosition;
            Clear();
            return (uncompressedSize, compressedSize);
        }
    }
}