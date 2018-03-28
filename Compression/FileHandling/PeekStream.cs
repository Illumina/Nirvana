using System;
using System.IO;

namespace Compression.FileHandling
{
    public sealed class PeekStream : Stream
    {
        private const int DefaultBufferSize = 4096;

        private Stream _stream;
        private byte[] _buffer;
        private readonly int _bufferSize;
        private int _readPos;
        private int _readLen;

        #region Stream

        private void EnsureNotClosed()
        {
            if (_stream == null) throw new ObjectDisposedException(null);
        }

        private void EnsureCanSeek()
        {
            if (!_stream.CanSeek) throw new NotSupportedException();
        }

        private void EnsureCanRead()
        {
            if (!_stream.CanRead) throw new NotSupportedException();
        }

        public override bool CanRead => _stream != null && _stream.CanRead;

        public override bool CanWrite => false;

        public override bool CanSeek => _stream != null && _stream.CanSeek;

        public override long Length
        {
            get
            {
                EnsureNotClosed();
                return _stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                EnsureNotClosed();
                EnsureCanSeek();
                return _stream.Position + (_readPos - _readLen);
            }
            set => throw new NotImplementedException();
        }

        public override int ReadByte()
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void WriteByte(byte value)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Flush() => EnsureNotClosed();

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            try
            {
                // ReSharper disable once InvertIf
                if (disposing && _stream != null)
                {
                    try
                    {
                        Flush();
                    }
                    finally
                    {
                        _stream.Dispose();
                    }
                }
            }
            finally
            {
                _stream = null;
                _buffer = null;

                base.Dispose(disposing);
            }
        }

        #endregion

        public PeekStream(Stream stream, int bufferSize = DefaultBufferSize)
        {
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));

            _stream     = stream ?? throw new ArgumentNullException(nameof(stream));
            _bufferSize = bufferSize;
            _buffer     = new byte[_bufferSize];

            _readLen = _stream.Read(_buffer, 0, _bufferSize);

            if (!_stream.CanRead && !_stream.CanWrite) throw new ObjectDisposedException(null);
        }

        private int ReadFromBuffer(byte[] array, int offset, int count)
        {
            int readbytes = _readLen - _readPos;

            if (readbytes == 0) return 0;

            if (readbytes > count) readbytes = count;
            Buffer.BlockCopy(_buffer, _readPos, array, offset, readbytes);
            _readPos += readbytes;

            return readbytes;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)                 throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)                     throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)                      throw new ArgumentOutOfRangeException(nameof(count));
            if (buffer.Length - offset < count) throw new ArgumentException($"The buffer is not large enough to read in an additional {count} bytes.");

            EnsureNotClosed();
            EnsureCanRead();

            int bytesFromBuffer = ReadFromBuffer(buffer, offset, count);
            if (bytesFromBuffer == count) return bytesFromBuffer;

            int alreadySatisfied = bytesFromBuffer;
            if (bytesFromBuffer > 0)
            {
                count -= bytesFromBuffer;
                offset += bytesFromBuffer;
            }

            _readPos = _readLen = 0;

            if (count >= _bufferSize)
            {
                return _stream.Read(buffer, offset, count) + alreadySatisfied;
            }

            _readLen = _stream.Read(_buffer, 0, _bufferSize);

            bytesFromBuffer = ReadFromBuffer(buffer, offset, count);
            return bytesFromBuffer + alreadySatisfied;
        }

        public byte[] PeekBytes(int numBytes)
        {
            if (numBytes < 0) throw new ArgumentOutOfRangeException(nameof(numBytes));
            var bytes = new byte[numBytes];
            Buffer.BlockCopy(_buffer, 0, bytes, 0, numBytes);
            return bytes;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureNotClosed();
            EnsureCanSeek();

            // The buffer is either empty or we have a buffered read.

            if (_readLen - _readPos > 0 && origin == SeekOrigin.Current)
            {
                // If we have bytes in the read buffer, adjust the seek offset to account for the resulting difference
                // between this stream's position and the underlying stream's position.            
                offset -= _readLen - _readPos;
            }

            long oldPos = Position;
            long newPos = _stream.Seek(offset, origin);

            // If the seek destination is still within the data currently in the buffer, we want to keep the buffer data and continue using it.
            // Otherwise we will throw away the buffer. This can only happen on read, as we flushed write data above.

            // The offset of the new/updated seek pointer within _buffer:
            _readPos = (int)(newPos - (oldPos - _readPos));

            // If the offset of the updated seek pointer in the buffer is still legal, then we can keep using the buffer:
            if (0 <= _readPos && _readPos < _readLen)
            {
                // Adjust the seek pointer of the underlying stream to reflect the amount of useful bytes in the read buffer:
                _stream.Seek(_readLen - _readPos, SeekOrigin.Current);
            }
            else
            {  // The offset of the updated seek pointer is not a legal offset. Loose the buffer.
                _readPos = _readLen = 0;
            }

            return newPos;
        }
    }
}