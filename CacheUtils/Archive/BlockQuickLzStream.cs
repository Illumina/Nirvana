using System;
using System.IO;
using System.IO.Compression;
using ErrorHandling.Exceptions;
using VariantAnnotation.Compression;
using VariantAnnotation.Utilities;

namespace CacheUtils.Archive
{
    public class BlockQuickLzStream : Stream
    {
        #region members

        private readonly bool _isCompressor;
        private readonly bool _leaveStreamOpen;

        private Stream _stream;

        private readonly string _filePath;
        private readonly QuickLzBlock _qlzBlock;
        private long _fileOffset;
        private bool _foundEof;

        #endregion

        #region Stream

        public override bool CanRead => _stream != null && _stream.CanRead;

        public override bool CanWrite => _stream != null && _stream.CanWrite;

        public override bool CanSeek => _stream != null && _stream.CanSeek;

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { return _fileOffset + _qlzBlock.Offset; }
            set { throw new NotSupportedException(); }
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
            if (_stream == null) throw new ObjectDisposedException($"BlockQuickLzStream ({_filePath}) has already been disposed.");
            _stream?.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && _stream != null)
                {
                    if (_isCompressor)
                    {
                        if (!_qlzBlock.IsFull) _qlzBlock.Write(_stream);
                        _qlzBlock.WriteEof(_stream);
                    }

                    if (!_leaveStreamOpen)
                    {
                        _stream.Dispose();
                        _stream = null;
                    }
                }
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
        /// <param name="stream">The stream to compress or decompress.</param>
        /// <param name="compressionMode">One of the enumeration values that indicates whether to compress or decompress the stream.</param>
        /// <param name="leaveStreamOpen">true to leave the stream open after disposing the object; otherwise, false.</param>
        public BlockQuickLzStream(Stream stream, CompressionMode compressionMode, bool leaveStreamOpen = false)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            if ((compressionMode != CompressionMode.Compress) && (compressionMode != CompressionMode.Decompress))
                throw new ArgumentOutOfRangeException(nameof(compressionMode));

            _stream          = stream;
            _isCompressor    = compressionMode == CompressionMode.Compress;
            _leaveStreamOpen = leaveStreamOpen;
            _filePath        = FileUtilities.GetPath(stream);
            _qlzBlock        = new QuickLzBlock();

            // sanity check: make sure we can use the stream for reading or writing
            if (_isCompressor && !_stream.CanWrite) throw new ArgumentException("A stream lacking write capability was provided to the block GZip compressor.");
            if (!_isCompressor && !_stream.CanRead) throw new ArgumentException("A stream lacking read capability was provided to the block GZip decompressor.");
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
            if(_stream == null) throw new ObjectDisposedException("Stream has already been disposed.");

            int numBytesRead = 0;
            int dataOffset = offset;

            while (numBytesRead < count)
            {
                if (!_qlzBlock.HasMoreData)
                {
                    var numBytes = _qlzBlock.Read(_stream);

                    if (numBytes == -1)
                    {
                        _foundEof = true;
                        return numBytesRead;
                    }

                    _fileOffset += numBytes;
                }

                int copyLength = _qlzBlock.CopyFrom(array, dataOffset, count - numBytesRead);

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
            if (_stream == null) throw new ObjectDisposedException($"Stream ({_filePath}) has already been disposed.");

            int numBytesWritten = 0;
            int dataOffset = offset;

            while (numBytesWritten < count)
            {
                int copyLength = _qlzBlock.CopyTo(array, dataOffset, count - numBytesWritten);
                dataOffset += copyLength;
                numBytesWritten += copyLength;

                if (_qlzBlock.IsFull) _qlzBlock.Write(_stream);
            }
        }
    }
}
