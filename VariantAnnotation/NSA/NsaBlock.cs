using System.Collections.Generic;
using System.IO;
using Compression.Algorithms;
using IO;

namespace VariantAnnotation.NSA
{
    public sealed class NsaBlock
    {
        private readonly ICompressionAlgorithm _compressionAlgorithm;
        private readonly byte[] _compressedBlock;
        public readonly byte[] UncompressedBlock;
        private int _compressedLength;
        private int _uncompressedLength;
        private readonly ExtendedBinaryWriter _writer;
        public int BlockOffset => (int)_writer.BaseStream.Position;
        private int _firstPosition;
        private int _lastPosition;
        private int _count;

        public int FirstPosition => _firstPosition;
        public int LastPosition => _lastPosition;
        public NsaBlock(ICompressionAlgorithm compressionAlgorithm, int size)
        {
            _compressionAlgorithm = compressionAlgorithm;
            UncompressedBlock = new byte[size];
            _writer = new ExtendedBinaryWriter(new MemoryStream(UncompressedBlock));
            int compressedBlockSize = compressionAlgorithm.GetCompressedBufferBounds(size);
            _compressedBlock = new byte[compressedBlockSize];
        }

        public int Read(ExtendedBinaryReader reader)
        {
            _compressedLength = reader.ReadOptInt32();
            _firstPosition = reader.ReadOptInt32();
            //_lastPosition = reader.ReadOptInt32();
            _count = reader.ReadOptInt32();
            reader.Read(_compressedBlock, 0, _compressedLength);

            _uncompressedLength = _compressionAlgorithm.Decompress(_compressedBlock, _compressedLength,
                UncompressedBlock, UncompressedBlock.Length);

            return _uncompressedLength;
        }


        public bool HasSpace(int length)
        {
            return BlockOffset + length + 2 * sizeof(int) <= UncompressedBlock.Length; //saving space for length and position
        }

        public void Add(byte[] data, int length, int position)
        {
            if (!HasSpace(length)) return;

            if (_writer.BaseStream.Position == 0)
            {
                _firstPosition = position;
                _lastPosition = position;
            }

            _writer.WriteOpt(length);
            _writer.WriteOpt(position - _lastPosition);
            _writer.Write(data, 0, length);

            _lastPosition = position;
            _count++;
        }

        public IEnumerable<(int position, byte[] data)> GetAnnotations()
        {
            if (_uncompressedLength == 0) yield break;
            _lastPosition = _firstPosition;
            using (var reader = new ExtendedBinaryReader(new MemoryStream(UncompressedBlock, 0, _uncompressedLength)))
            {
                for (int i = 0; i < _count; i++)
                {
                    var length = reader.ReadOptInt32();
                    _lastPosition += reader.ReadOptInt32();

                    var data = reader.ReadBytes(length);

                    yield return (_lastPosition, data);
                }

            }
        }

        public (int firstPosition, int lastPosition, int numBytes) Write(ExtendedBinaryWriter writer)
        {
            var compressedLength = _compressionAlgorithm.Compress(UncompressedBlock, BlockOffset,
                _compressedBlock, _compressedBlock.Length);

            writer.WriteOpt(compressedLength);
            writer.WriteOpt(_firstPosition);
            //writer.WriteOpt(_lastPosition);
            writer.WriteOpt(_count);
            writer.Write(_compressedBlock, 0, compressedLength);

            _writer.BaseStream.Position = 0;

            return (_firstPosition, _lastPosition, compressedLength);
        }

        public void Clear()
        {
            _count = 0;
            _firstPosition = -1;
            _lastPosition = -1;
            _compressedLength = 0;
            _uncompressedLength = 0;
        }
    }
}