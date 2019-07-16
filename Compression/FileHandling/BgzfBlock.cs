using System;
using System.IO;
using System.Text;
using Compression.Algorithms;
using ErrorHandling.Exceptions;

namespace Compression.FileHandling
{
    public sealed class BgzfBlock
    {
        private const int MaxBlockSize             = 65536;
        private readonly byte[] _compressedBlock   = new byte[MaxBlockSize];
        private readonly byte[] _uncompressedBlock = new byte[MaxBlockSize];
        private readonly Zlib _bgzf                = new Zlib();

        public string Read(Stream stream)
        {
            int count = stream.Read(_compressedBlock, 0, BlockGZipStream.BlockGZipFormatCommon.BlockHeaderLength);
            if (count == 0) return string.Empty;

            if (!BlockGZipStream.HasValidHeader(count, _compressedBlock))
                throw new InvalidDataException("Found an invalid header when reading the GZip block");

            int blockLength = BitConverter.ToUInt16(_compressedBlock, 16) + 1;
            int remaining   = blockLength - BlockGZipStream.BlockGZipFormatCommon.BlockHeaderLength;

            count = stream.Read(_compressedBlock, BlockGZipStream.BlockGZipFormatCommon.BlockHeaderLength, remaining);

            if (count != remaining) throw new InvalidDataException("Found unexpected truncation when reading the GZip block");

            count = _bgzf.Decompress(_compressedBlock, blockLength, _uncompressedBlock, MaxBlockSize);

            if (count < 0) throw new CompressionException("Encountered an error when uncompressing the GZip block");
            return Encoding.UTF8.GetString(_uncompressedBlock, 0, count);
        }
    }
}
