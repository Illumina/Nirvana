using System;
using System.IO;
using System.IO.Compression;
using Compression.Algorithms;
using ErrorHandling.Exceptions;

namespace Compression.FileHandling
{
    // BGZF/GZIP header (specialized from RFC 1952; little endian):
    // +---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+
    // | 31|139|  8|  4|              0|  0|255|      6| 66| 67|      2|BLK_LEN|
    // +---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+

    public sealed class BlockGZipStream : Stream
    {
        #region members

        private readonly byte[] _compressedBlock;
        private readonly byte[] _uncompressedBlock;
        private int _blockOffset;
        private int _blockLength;
        private long _blockAddress;

        private readonly bool _isCompressor;
        private readonly bool _leaveStreamOpen;

        private readonly string _filePath;
        private Stream _stream;
        private readonly Zlib _bgzf;
        private bool _isDisposed;

        public static class BlockGZipFormatCommon
        {
            #region members

            public const int BlockSize         = 65280;
            public const int MaxBlockSize      = 65536;
            public const int BlockHeaderLength = 18;

            #endregion
        }

        #endregion

        #region Stream

        public override bool CanRead => _stream != null && !_isCompressor && _stream.CanRead;

        public override bool CanWrite => _stream != null && _isCompressor && _stream.CanWrite;

        public override bool CanSeek => _stream != null && !_isCompressor && _stream.CanSeek;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => (_blockAddress << 16) | ((long)_blockOffset & 0xffff);
            set => SeekVirtualFilePointer((ulong)value);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            try
            {
                if (_isCompressor)
                {
                    Flush(_blockOffset);

                    // write an empty block (as EOF marker)
                    Flush(0);
                }

                if (!_leaveStreamOpen)
                {
                    _stream.Dispose();
                    _stream = null;
                }

                _isDisposed = true;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        #endregion

        /// <summary>
        /// private constructor
        /// </summary>
        private BlockGZipStream(CompressionMode compressionMode, int compressionLevel)
        {
            _bgzf = new Zlib(compressionLevel);
            _isCompressor = compressionMode == CompressionMode.Compress;

            _uncompressedBlock = new byte[BlockGZipFormatCommon.MaxBlockSize];
            _compressedBlock   = new byte[_bgzf.GetCompressedBufferBounds(BlockGZipFormatCommon.MaxBlockSize)];            
        }

        /// <summary>
        /// stream constructor
        /// </summary>
        public BlockGZipStream(Stream stream, CompressionMode compressionMode, bool leaveStreamOpen = false, int compressionLevel = 1)
            : this(compressionMode, compressionLevel)
        {
            _filePath        = "(stream)";
            _leaveStreamOpen = leaveStreamOpen;
            _stream          = stream;

            // sanity check: make sure the stream exists
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            // sanity check: make sure we can use the stream for reading or writing
            if (_isCompressor  && !_stream.CanWrite) throw new CompressionException("A stream lacking write capability was provided to the block GZip compressor.");
            if (!_isCompressor && !_stream.CanRead)  throw new CompressionException("A stream lacking read capability was provided to the block GZip decompressor.");
        }

        /// <summary>
        /// flushes the BGZF stream
        /// </summary>
        private void Flush(int uncompressedSize)
        {
            int blockLength = _bgzf.Compress(_uncompressedBlock, uncompressedSize, _compressedBlock, BlockGZipFormatCommon.MaxBlockSize);
            _blockOffset    = 0;

            _stream.Write(_compressedBlock, 0, blockLength);
			_blockAddress = _stream.Position;	
        }

        /// <summary>
        /// returns true if this GZip block has a valid header
        /// </summary>
        private static bool HasValidHeader(int numHeaderBytes, byte[] header)
        {
            if (numHeaderBytes != BlockGZipFormatCommon.BlockHeaderLength) return false;

            return header[0] == 31      &&
                   header[1] == 139     &&
                   header[2] == 8       &&
                   (header[3] & 4) != 0 &&
                   header[12] == 66     &&
                   header[13] == 67;
        }

        /// <summary>
        /// reads the next GZip block from the disk
        /// </summary>
        private void ReadBlock()
        {
            var header = new byte[BlockGZipFormatCommon.BlockHeaderLength];
            long blockAddress = _stream.CanSeek ? _stream.Position : 0;
            int count  = _stream.Read(header, 0, BlockGZipFormatCommon.BlockHeaderLength);

            // handle the case where no data was read
            if (count == 0)
            {
                _blockLength = 0;
                return;
            }

            // check the header
            if (!HasValidHeader(count, header))
            {
                throw new CompressionException($"Found an invalid header when reading the GZip block ({_filePath})");
            }

            int blockLength = BitConverter.ToUInt16(header, 16) + 1;
            int remaining   = blockLength - BlockGZipFormatCommon.BlockHeaderLength;

            Buffer.BlockCopy(header, 0, _compressedBlock, 0, BlockGZipFormatCommon.BlockHeaderLength);

            count = _stream.Read(_compressedBlock, BlockGZipFormatCommon.BlockHeaderLength, remaining);

            // handle unexpected truncation
            if (count != remaining)
            {
                throw new CompressionException($"Found unexpected truncation when reading the GZip block ({_filePath})");
            }

            count = _bgzf.Decompress(_compressedBlock, blockLength, _uncompressedBlock, BlockGZipFormatCommon.MaxBlockSize);

            if (count < 0)
            {
                throw new CompressionException($"Encountered an error when uncompressing the GZip block ({_filePath})");
            }

            // Do not reset offset if this read follows a seek
            if (_blockLength != 0) _blockOffset = 0;

            _blockAddress = blockAddress;
            _blockLength  = count;
        }

        /// <summary>
        /// reads from the disk into the byte array
        /// </summary>
        public override int Read(byte[] data, int offset, int numBytesToRead)
        {
            if (_isCompressor) throw new CompressionException("Tried to read data from a compression BlockGZipStream.");

            if (numBytesToRead == 0) return 0;

            int numBytesRead = 0;
            int dataOffset   = offset;

            while (numBytesRead < numBytesToRead)
            {
                int numBytesAvailable = _blockLength - _blockOffset;

                if (numBytesAvailable <= 0)
                {
                    ReadBlock();
                    numBytesAvailable = _blockLength - _blockOffset;
                    if (numBytesAvailable <= 0) break;
                }

                int copyLength = Math.Min(numBytesToRead - numBytesRead, numBytesAvailable);
                Buffer.BlockCopy(_uncompressedBlock, _blockOffset, data, dataOffset, copyLength);

                _blockOffset += copyLength;
                dataOffset   += copyLength;
                numBytesRead += copyLength;
            }

            if (_blockOffset == _blockLength)
            {
                _blockAddress = _stream.CanSeek ? _stream.Position : 0;
                _blockOffset  = _blockLength = 0;
            }

            return numBytesRead;
        }

        /// <summary>
        /// writes the byte array to disk. Returns the number of bytes written
        /// </summary>
        public override void Write(byte[] data, int offset, int numBytesToWrite)
        {
            if (!_isCompressor) throw new CompressionException("Tried to write data to a decompression BlockGZipStream.");

            int numBytesWritten = 0;
            int dataOffset      = offset;

            // copy the data to the buffer
            while (numBytesWritten < numBytesToWrite)
            {
                int copyLength = Math.Min(BlockGZipFormatCommon.BlockSize - _blockOffset, numBytesToWrite - numBytesWritten);
                Buffer.BlockCopy(data, dataOffset, _uncompressedBlock, _blockOffset, copyLength);

                _blockOffset    += copyLength;
                dataOffset      += copyLength;
                numBytesWritten += copyLength;

                if (_blockOffset == BlockGZipFormatCommon.BlockSize) Flush(_blockOffset);
            }
        }

		/// <summary>
		/// Method to seek to a virtual position within the filestream.
		/// </summary>
		/// <param name="virtualPosition">Virtual positions are comprised of the first 48 bits being the file byte offset
		/// and the last 16 bits being the offset within the uncompressed data block.</param>
		private void SeekVirtualFilePointer(ulong virtualPosition)
		{
			var compressedOffset = GetCompressedOffset(virtualPosition);
			var uncompressedOffset = GetUncompressedOffset(virtualPosition);
			// if we're already in the right block, no need to reload buffer.
			if (_blockAddress != compressedOffset)
			{
				_blockAddress = compressedOffset;
				_stream.Position = _blockAddress;
				ReadBlock();
			}
			_blockOffset = uncompressedOffset;
		}
		/// <summary>
		/// Extract the file byte offset from the Virtual Position.
		/// </summary>
		/// <param name="virtualPosition">64-bit number with the first 48 bits being the position to extract.</param>
		/// <returns>A long file byte offset.</returns>
		private static long GetCompressedOffset(ulong virtualPosition)
		{
			unchecked
			{
				return (long)((virtualPosition >> 16) & 0xFFFFFFFFFFFFL);
			}
		}

		/// <summary>
		/// Extract the offset within the uncompressed block from the Virtual Position.
		/// </summary>
		/// <param name="virtualPosition">64-bit number with the last 16 bits being the position to extract.</param>
		/// <returns>An int offset of the uncompressed block.</returns>
		private static int GetUncompressedOffset(ulong virtualPosition)
		{
			unchecked
			{
				return (int)(virtualPosition & 0xffff);
			}
		}
	}
}
