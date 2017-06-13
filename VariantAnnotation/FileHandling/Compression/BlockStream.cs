using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using ErrorHandling.Exceptions;

namespace VariantAnnotation.FileHandling.Compression
{
    public sealed class BlockStream : Stream
    {
        #region members

        private readonly bool _isCompressor;
        private readonly bool _leaveStreamOpen;

        private Stream _stream;
        private BinaryWriter _writer;
        private IFileHeader _header;

        private readonly Block _block;
        private bool _foundEof;
        private bool _isDisposed;

		#endregion

		#region Stream

		public override bool CanRead => _stream.CanRead;

        public override bool CanWrite => _stream.CanWrite;

        public override bool CanSeek => _stream.CanSeek;

        public override long Length => throw new NotSupportedException();

	    public override long Position
        {
            get => _stream.Position;
		    set => throw new NotSupportedException();
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
            if (_block.Offset > 0) _block.Write(_stream);
        }

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            try
            {
                if (_isCompressor)
                {
                    Flush();
                    _block.WriteEof(_stream);

                    // update the header
                    if (_header != null)
                    {
                        _stream.Position = 0;
                        WriteHeader(_header);
                    }

                    _writer.Dispose();
                    _writer = null;
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
        /// constructor
        /// </summary>
        /// <param name="compressionAlgorithm">The algorithm used for compression and decompression.</param>
        /// <param name="stream">The stream to compress or decompress.</param>
        /// <param name="compressionMode">One of the enumeration values that indicates whether to compress or decompress the stream.</param>
        /// <param name="leaveStreamOpen">true to leave the stream open after disposing the object; otherwise, false.</param>
        /// <param name="size">The size of the compression block</param>
        public BlockStream(ICompressionAlgorithm compressionAlgorithm, Stream stream, CompressionMode compressionMode,
            bool leaveStreamOpen = false, int size = 16777216)
        {
	        _stream          = stream ?? throw new ArgumentNullException(nameof(stream));
            _isCompressor    = compressionMode == CompressionMode.Compress;
            _leaveStreamOpen = leaveStreamOpen;
            _block           = new Block(compressionAlgorithm, size);

            // sanity check: make sure we can use the stream for reading or writing
            if (_isCompressor && !_stream.CanWrite) throw new ArgumentException("A stream lacking write capability was provided to the block GZip compressor.");
            if (!_isCompressor && !_stream.CanRead) throw new ArgumentException("A stream lacking read capability was provided to the block GZip decompressor.");

            if (_isCompressor) _writer = new BinaryWriter(_stream, Encoding.UTF8, true);
        }

        public void WriteHeader(IFileHeader header)
        {
            _header = header;
            header.Write(_writer);
        }
        
        public IFileHeader ReadHeader(IFileHeader tempHeader)
        {
            IFileHeader header;

            using (var reader = new BinaryReader(_stream, Encoding.UTF8, true))
            {
                header = tempHeader.Read(reader);
            }

            return header;
        }

        /// <summary>
        /// Reads a number of decompressed bytes into the specified byte array.
        /// </summary>
        /// <param name="array">The array to store decompressed bytes.</param>
        /// <param name="offset">The byte offset in <paramref name="array"/> at which the read bytes will be placed.</param>
        /// <param name="count">The maximum number of decompressed bytes to read.</param>
        /// <returns>The number of bytes that were read into the byte array.</returns>
        public override int Read(byte[] array, int offset, int count)
        {
            if (_foundEof) return 0;
            if (_isCompressor) throw new CompressionException("Tried to read data from a compression BlockGZipStream.");

            ValidateParameters(array, offset, count);

            int numBytesRead = 0;
            int dataOffset = offset;

            while (numBytesRead < count)
            {
                if (!_block.HasMoreData)
                {
                    var numBytes = _block.Read(_stream);

                    if (numBytes == -1)
                    {
                        _foundEof = true;
                        return numBytesRead;
                    }
                }

                int copyLength = _block.CopyFrom(array, dataOffset, count - numBytesRead);

                dataOffset += copyLength;
                numBytesRead += copyLength;
            }

            return numBytesRead;
        }

        private void ValidateParameters(byte[] array, int offset, int count)
        {
            if (array == null)                 throw new ArgumentNullException(nameof(array));
            if (offset < 0)                    throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)                     throw new ArgumentOutOfRangeException(nameof(count));
            if (array.Length - offset < count) throw new ArgumentException("Invalid Argument Offset Count");
        }

        /// <summary>
        /// Writes compressed bytes to the underlying stream from the specified byte array.
        /// </summary>
        /// <param name="array">The buffer that contains the data to compress.</param>
        /// <param name="offset">The byte offset in <paramref name="array"/> from which the bytes will be read.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        public override void Write(byte[] array, int offset, int count)
        {
            if (!_isCompressor) throw new CompressionException("Tried to write data to a decompression BlockGZipStream.");

            ValidateParameters(array, offset, count);

            int numBytesWritten = 0;
            int dataOffset = offset;

            while (numBytesWritten < count)
            {
                int copyLength = _block.CopyTo(array, dataOffset, count - numBytesWritten);
                dataOffset += copyLength;
                numBytesWritten += copyLength;
                if (_block.IsFull) _block.Write(_stream);
            }
        }

        public class BlockPosition
        {
            public long FileOffset;
            public int InternalOffset;
        }

        /// <summary>
        /// retrieves the current position of the block
        /// </summary>
        public void GetBlockPosition(BlockPosition bp)
        {
            bp.FileOffset     = _stream.Position;
            bp.InternalOffset = _block.Offset;
        }

        public void SetBlockPosition(BlockPosition bp)
        {
            if (bp.FileOffset != _block.FileOffset)
            {
                _stream.Position = bp.FileOffset;
                _block.Read(_stream);
            }

            _block.Offset = bp.InternalOffset;
		}
    }
}
