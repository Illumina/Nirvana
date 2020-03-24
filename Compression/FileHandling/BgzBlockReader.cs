using System;
using System.IO;
using ErrorHandling.Exceptions;

namespace Compression.FileHandling
{
    public sealed class BgzBlockReader:IDisposable
    {
        private readonly string _filePath;
        private readonly Stream _stream;
        private readonly bool _leaveStreamOpen;
        
        public long Position => _stream.Position;

        public BgzBlockReader(Stream stream, bool leaveStreamOpen = false)
        {
            _filePath = "(stream)";
            _stream = stream;
            _leaveStreamOpen = leaveStreamOpen;
        }
        
        //read the next compressed block into provided buffer
        public int ReadCompressedBlock(byte[] buffer)
        {
            if (buffer.Length < BlockGZipStream.BlockGZipFormatCommon.MaxBlockSize)
                throw new InsufficientMemoryException($"Pease provide a buffer at least {BlockGZipStream.BlockGZipFormatCommon.MaxBlockSize} bytes in size.");
            int headerSize = _stream.Read(buffer, 0, BlockGZipStream.BlockGZipFormatCommon.BlockHeaderLength);

            // handle the case where no data was read
            if (headerSize == 0) return 0;
            
            // check the header
            if (!BlockGZipStream.HasValidHeader(headerSize, buffer))
            {
                throw new CompressionException($"Found an invalid header when reading the GZip block ({_filePath})");
            }

            int blockLength = BitConverter.ToUInt16(buffer, 16) + 1;
            int expectedDataSize   = blockLength  - BlockGZipStream.BlockGZipFormatCommon.BlockHeaderLength;

            var dataSize = _stream.Read(buffer, BlockGZipStream.BlockGZipFormatCommon.BlockHeaderLength, expectedDataSize);

            // handle unexpected truncation
            if (expectedDataSize != dataSize)
            {
                throw new CompressionException($"Found unexpected truncation when reading the GZip block ({_filePath})");
            }

            return headerSize+dataSize;
        }

        public void Dispose()
        {
            if (_leaveStreamOpen) return;
            _stream?.Dispose();
        }
    }
}