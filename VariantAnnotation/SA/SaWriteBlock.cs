using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Compression.Algorithms;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace VariantAnnotation.SA
{
    public sealed class SaWriteBlock : SaBlock
    {
        private readonly List<ISaIndexOffset> _blockPositions;
        internal int BlockOffset;

        public const int DefaultBlockSize = 524288 * 8;

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

        public (int FirstPosition, int LastPosition) Write(Stream stream)
        {
            WriteHeader(stream);

            using (var writer = new ExtendedBinaryWriter(stream, Encoding.ASCII, true))
            {
                WriteBlockOffsets(writer);
            }

            if (Header.NumCompressedBytes > Header.NumUncompressedBytes) WriteUncompressedBlock(stream);
            else WriteCompressedBlock(stream);

            BlockOffset = 0;

            var firstPosition = _blockPositions[0].Position;
            var lastPosition = _blockPositions[_blockPositions.Count - 1].Position;
            _blockPositions.Clear();

            return (firstPosition, lastPosition);
        }

        private void WriteBlockOffsets(IExtendedBinaryWriter writer)
        {
            writer.WriteOpt(_blockPositions.Count);

            int oldPosition = 0;
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