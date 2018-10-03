using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Compression.Algorithms;
using IO;
using VariantAnnotation.Interface.SA;

namespace VariantAnnotation.SA
{
    public sealed class SaWriteBlock : SaBlock
    {
        private readonly List<ISaIndexOffset> _blockPositions;
        public int BlockOffset { get; private set; }

        public const int DefaultBlockSize = 4_194_304;

        public SaWriteBlock(ICompressionAlgorithm compressionAlgorithm, int size = DefaultBlockSize)
            : base(compressionAlgorithm, size)
        {
            _blockPositions = new List<ISaIndexOffset>();
        }

        public bool HasSpace(int numBytes) => BlockOffset + numBytes < UncompressedBlock.Length;

        public void Add(byte[] source, int position)
        {
            _blockPositions.Add(new SaIndexOffset(position, BlockOffset));

            Buffer.BlockCopy(source, 0, UncompressedBlock, BlockOffset, source.Length);
            BlockOffset += source.Length;
        }

        public void Add(byte[] buffer, int length, int position)
        {
            _blockPositions.Add(new SaIndexOffset(position, BlockOffset));

            Buffer.BlockCopy(buffer, 0, UncompressedBlock, BlockOffset, length);
            BlockOffset += length;
        }

        public (int FirstPosition, int LastPosition,int numBytes) Write(Stream stream)
        {
            long initialPosition = stream.Position;
            WriteHeader(stream);

            using (var writer = new ExtendedBinaryWriter(stream, Encoding.UTF8, true))
            {
                WriteBlockOffsets(writer);
            }

            int numBytes = (int)(stream.Position - initialPosition);
            if (Header.NumCompressedBytes > Header.NumUncompressedBytes)
            {
                WriteUncompressedBlock(stream);
                numBytes += Header.NumUncompressedBytes;
            }
            else
            {
                WriteCompressedBlock(stream);
                numBytes += Header.NumCompressedBytes;
            }

            BlockOffset = 0;

            int firstPosition = _blockPositions[0].Position;
            int lastPosition = _blockPositions[_blockPositions.Count - 1].Position;
            _blockPositions.Clear();

            return (firstPosition, lastPosition, numBytes);
        }

        private void WriteBlockOffsets(IExtendedBinaryWriter writer)
        {
            writer.WriteOpt(_blockPositions.Count);

            var oldPosition = 0;
            long oldBlockOffset = 0;

            foreach (var offset in _blockPositions.OrderBy(x => x.Position))
            {
                int deltaPosition = offset.Position - oldPosition;
                long deltaBlockOffset = offset.Offset - oldBlockOffset;

                writer.WriteOpt(deltaPosition);
                writer.WriteOpt(deltaBlockOffset);

                oldPosition = offset.Position;
                oldBlockOffset = offset.Offset;
            }
        }

        private void WriteHeader(Stream stream)
        {
            Header.NumUncompressedBytes = BlockOffset;
            Header.NumCompressedBytes = CompressionAlgorithm.Compress(UncompressedBlock, Header.NumUncompressedBytes, CompressedBlock, CompressedBlock.Length);
            Header.Write(stream);
        }

        private void WriteCompressedBlock(Stream outputStream) => outputStream.Write(CompressedBlock, 0, Header.NumCompressedBytes);

        private void WriteUncompressedBlock(Stream outputStream) => outputStream.Write(UncompressedBlock, 0, Header.NumUncompressedBytes);
    }
}