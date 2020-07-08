using System;
using System.Collections.Generic;
using System.IO;
using Compression.Algorithms;
using IO;

namespace VariantAnnotation.NSA
{
    public sealed class NsaBlock:IDisposable
    {
        private readonly ICompressionAlgorithm _compressionAlgorithm;
        private readonly byte[] _compressedBlock;
        private readonly byte[] _uncompressedBlock;
        private int _compressedLength;
        private int _uncompressedLength;
        private readonly ExtendedBinaryWriter _writer;
        public int BlockOffset => (int)_writer.BaseStream.Position;
        private int _firstPosition;
        private int _lastPosition;
        private int _count;
        
        private readonly ExtendedBinaryReader _blockReader;
        private readonly MemoryStream         _blockStream;
        
        
        public NsaBlock(ICompressionAlgorithm compressionAlgorithm, int size)
        {
            _compressionAlgorithm = compressionAlgorithm;
            _uncompressedBlock    = new byte[size];
            _blockStream          = new MemoryStream(_uncompressedBlock);
            _blockReader          = new ExtendedBinaryReader(_blockStream);
            _writer               = new ExtendedBinaryWriter(new MemoryStream(_uncompressedBlock));
            
            int compressedBlockSize = compressionAlgorithm.GetCompressedBufferBounds(size);
            _compressedBlock = new byte[compressedBlockSize];
            
        }

        public void Read(ExtendedBinaryReader reader)
        {
            _compressedLength = reader.ReadOptInt32();
            _firstPosition    = reader.ReadOptInt32();
            //_lastPosition   = reader.ReadOptInt32();
            _count            = reader.ReadOptInt32();
            reader.Read(_compressedBlock, 0, _compressedLength);

            _uncompressedLength = _compressionAlgorithm.Decompress(_compressedBlock, _compressedLength,
                _uncompressedBlock, _uncompressedBlock.Length);
            
            _blockStream.Position = 0;
        }

        //read block but do not uncompress
        public void ReadCompressedBytes(ExtendedBinaryReader reader)
        {
            _compressedLength = reader.ReadOptInt32();
            _firstPosition    = reader.ReadOptInt32();
            //_lastPosition   = reader.ReadOptInt32();
            _count            = reader.ReadOptInt32();
            reader.Read(_compressedBlock, 0, _compressedLength);

        }

        //write a block that has not been uncompressed
        public void WriteCompressedBytes(ExtendedBinaryWriter writer)
        {
            writer.WriteOpt(_compressedLength);
            writer.WriteOpt(_firstPosition);
            //writer.WriteOpt(_lastPosition);
            writer.WriteOpt(_count);
            writer.Write(_compressedBlock, 0, _compressedLength);

        }

        public bool HasSpace(int length)
        {
            return BlockOffset + length + 2 * sizeof(int) <= _uncompressedBlock.Length; //saving space for length and position
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

        
        public int AddAnnotations(List<int> vcfPositions, int j, List<AnnotationItem> annotationItems)
        {
            if (_uncompressedLength == 0) return j;

            _blockStream.Position = 0;
            var position = _firstPosition;

            var i = 0;
            var length = _blockReader.ReadOptInt32();
            position += _blockReader.ReadOptInt32();

            while (i < _count && j < vcfPositions.Count)
            {
                if (position < vcfPositions[j])
                {
                    _blockStream.Position += length;
                    //this position is not needed, move to next
                    length   =  _blockReader.ReadOptInt32();
                    position += _blockReader.ReadOptInt32();
                    i++;
                    continue;
                }

                if (vcfPositions[j] < position)
                {
                    //go to next position from vcf
                    j++;
                    continue;
                }
                
                //positions have matched
                var data = _blockReader.ReadBytes(length);
                
                annotationItems.Add(new AnnotationItem(position, data));

                j++;
                i++;
                length   =  _blockReader.ReadOptInt32();
                position += _blockReader.ReadOptInt32();
            }
            return j;
        }
        
        public (int firstPosition, int lastPosition, int numBytes) Write(ExtendedBinaryWriter writer)
        {
            var compressedLength = _compressionAlgorithm.Compress(_uncompressedBlock, BlockOffset,
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
            _blockStream.Position = 0;
        }
        
        public void Dispose()
        {
            _writer?.Dispose();
            _blockReader?.Dispose();
            _blockStream?.Dispose();
        }
    }
}