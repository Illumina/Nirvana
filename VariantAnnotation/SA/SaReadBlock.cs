using System.IO;
using System.Text;
using Compression.Algorithms;
using ErrorHandling.Exceptions;
using IO;
using VariantAnnotation.Interface.SA;

namespace VariantAnnotation.SA
{
    public sealed class SaReadBlock : SaBlock
    {
        private ISaIndexOffset[] _blockOffsets;
        public int FirstPosition => _blockOffsets[0].Position;
        public int LastPosition => _blockOffsets[_blockOffsets.Length - 1].Position;
        private readonly MemoryStream _stream;
        private readonly ExtendedBinaryReader _reader;

        public SaReadBlock(ICompressionAlgorithm compressionAlgorithm, int size = SaWriteBlock.DefaultBlockSize)
            : base(compressionAlgorithm, size)
        {
            _stream = new MemoryStream(UncompressedBlock);
            _reader = new ExtendedBinaryReader(_stream);
        }

        public void Read(Stream inputStream)
        {
            Header.Read(inputStream);
            if (Header.IsEmpty) throw new IOException("Unexpected empty header found while reading SaBlock header.");

            using (var reader = new ExtendedBinaryReader(inputStream, Encoding.UTF8, true))
            {
                ReadBlockOffsets(reader);
            }

            if (Header.NumCompressedBytes > Header.NumUncompressedBytes) ReadUncompressedBlock(inputStream);
            else ReadCompressedBlock(inputStream);
        }

        private void ReadBlockOffsets(ExtendedBinaryReader reader)
        {
            int numEntries = reader.ReadOptInt32();
            _blockOffsets  = new ISaIndexOffset[numEntries];

            var oldPosition    = 0;
            var oldBlockOffset = 0;

            for (var i = 0; i < numEntries; i++)
            {
                int deltaPosition    = reader.ReadOptInt32();
                int deltaBlockOffset = reader.ReadOptInt32();

                int position    = oldPosition + deltaPosition;
                int blockOffset = oldBlockOffset + deltaBlockOffset;

                _blockOffsets[i] = new SaIndexOffset(position, blockOffset);

                oldPosition = position;
                oldBlockOffset = blockOffset;
            }
        }

        private void ReadCompressedBlock(Stream inputStream)
        {
            int numBytesRead = inputStream.Read(CompressedBlock, 0, Header.NumCompressedBytes);
            if (numBytesRead != Header.NumCompressedBytes)
            {
                throw new IOException($"Expected {Header.NumCompressedBytes} bytes from the block, but received only {numBytesRead} bytes.");
            }

            int numUncompressedBytes = CompressionAlgorithm.Decompress(CompressedBlock, Header.NumCompressedBytes, UncompressedBlock, UncompressedBlock.Length);
            if (numUncompressedBytes != Header.NumUncompressedBytes)
            {
                throw new CompressionException($"Expected {Header.NumUncompressedBytes} bytes after decompression, but found only {numUncompressedBytes} bytes.");
            }
        }

        private void ReadUncompressedBlock(Stream inputStream)
        {
            int numUncompressedBytes = inputStream.Read(UncompressedBlock, 0, Header.NumUncompressedBytes);
            if (numUncompressedBytes != Header.NumUncompressedBytes)
            {
                throw new IOException($"Expected {Header.NumUncompressedBytes} bytes from the uncompressed block, but received only {numUncompressedBytes} bytes.");
            }
        }

        public int GetBlockOffset(int position)
        {
            int index = BinarySearch(position);
            return index < 0 ? -1 : _blockOffsets[index].Offset;
        }

        public ExtendedBinaryReader GetAnnotationReader(int position)
        {
            var offset = GetBlockOffset(position);
            if (offset < 0) return null;

            _stream.Position = offset;
            return _reader;
        }

        /// <summary>
        /// returns the index of the desired element, otherwise it returns a negative number
        /// </summary>
        private int BinarySearch(int position)
        {
            var begin = 0;
            int end = _blockOffsets.Length - 1;

            while (begin <= end)
            {
                int index = begin + (end - begin >> 1);

                int ret = _blockOffsets[index].Position.CompareTo(position);
                if (ret == 0) return index;
                if (ret < 0) begin = index + 1;
                else end = index - 1;
            }

            return ~begin;
        }
    }
}