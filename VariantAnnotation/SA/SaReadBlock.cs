using System.IO;
using System.Text;
using Compression.Algorithms;
using ErrorHandling.Exceptions;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace VariantAnnotation.SA
{
    public sealed class SaReadBlock : SaBlock
    {
        private ISaIndexOffset[] _blockOffsets;

        /// <summary>
        /// constructor
        /// </summary>
        public SaReadBlock(ICompressionAlgorithm compressionAlgorithm, int size = SaWriteBlock.DefaultBlockSize)
            : base(compressionAlgorithm, size)
        { }

        public void Read(Stream inputStream)
        {
            Header.Read(inputStream);
            if (Header.IsEmpty) throw new IOException("Unexpected empty header found while reading SaBlock header.");

            using (var reader = new ExtendedBinaryReader(inputStream, Encoding.ASCII, true))
            {
                ReadBlockOffsets(reader);
            }

            if (Header.NumCompressedBytes > Header.NumUncompressedBytes) ReadUncompressedBlock(inputStream);
            else ReadCompressedBlock(inputStream);
        }

        private void ReadBlockOffsets(IExtendedBinaryReader reader)
        {
            var numEntries = reader.ReadOptInt32();
            _blockOffsets = new ISaIndexOffset[numEntries];

            int oldPosition = 0;
            int oldBlockOffset = 0;

            for (int i = 0; i < numEntries; i++)
            {
                var deltaPosition = reader.ReadOptInt32();
                var deltaBlockOffset = reader.ReadOptInt32();

                var position = oldPosition + deltaPosition;
                var blockOffset = oldBlockOffset + deltaBlockOffset;

                _blockOffsets[i] = new SaIndexOffset(position, blockOffset);

                oldPosition = position;
                oldBlockOffset = blockOffset;
            }
        }

        private void ReadCompressedBlock(Stream inputStream)
        {
            var numBytesRead = inputStream.Read(CompressedBlock, 0, Header.NumCompressedBytes);
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
            var numUncompressedBytes = inputStream.Read(UncompressedBlock, 0, Header.NumUncompressedBytes);
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

        /// <summary>
        /// returns the index of the desired element, otherwise it returns a negative number
        /// </summary>
        private int BinarySearch(int position)
        {
            var begin = 0;
            var end = _blockOffsets.Length - 1;

            while (begin <= end)
            {
                var index = begin + (end - begin >> 1);

                var ret = _blockOffsets[index].Position.CompareTo(position);
                if (ret == 0) return index;
                if (ret < 0) begin = index + 1;
                else end = index - 1;
            }

            return ~begin;
        }
    }
}