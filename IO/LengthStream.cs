using System;
using System.IO;

namespace IO
{
    /// <inheritdoc />
    /// <summary>
    /// The S3 PutObjectRequest object requires an input stream that supports length and position.
    /// Neither of these are typically available from the CryptoStream
    /// </summary>
    public sealed class LengthStream : Stream
    {
        private readonly Stream _stream;
        private long _position;

        public LengthStream(Stream stream, long length)
        {
            _stream = stream;
            Length  = length;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            _position += count;
            return _stream.Read(buffer, offset, count);
        }

        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException();
        }

        public override void Flush()                                     => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin)        => throw new NotSupportedException();
        public override void SetLength(long value)                       => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override bool CanRead  => _stream.CanRead;
        public override bool CanSeek  => _stream.CanSeek;
        public override bool CanWrite => _stream.CanWrite;
        public override long Length { get; }
    }
}
