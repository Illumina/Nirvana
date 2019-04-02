using System;
using System.IO;
using System.Security.Cryptography;

namespace IO
{
    public sealed class MD5Stream : Stream
    {
        private readonly Stream _stream;
        private readonly IncrementalHash _md5 = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
        private FileMetadata _metadata;
        private long _length;

        /// <inheritdoc />
        public MD5Stream(Stream stream) => _stream = stream;

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
            _md5.AppendData(buffer, offset, count);
            _length += count;
        }

        public FileMetadata GetFileMetadata()
        {
            if (_metadata != null) return _metadata;
            _metadata = new FileMetadata(_md5.GetHashAndReset(), _length);
            return _metadata;
        }

        public override long Position
        {
            get => _length;
            set => throw new NotSupportedException();
        }

        public override void Flush() => _stream.Flush();

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin)      => throw new NotSupportedException();
        public override void SetLength(long value)                     => throw new NotSupportedException();
        public override bool CanRead  => _stream.CanRead;
        public override bool CanSeek  => _stream.CanSeek;
        public override bool CanWrite => _stream.CanWrite;
        public override long Length   => _stream.Length;
    }

    public sealed class FileMetadata
    {
        public byte[] MD5 { get; }
        public long Length { get; }

        public FileMetadata(byte[] md5, long length)
        {
            MD5    = md5;
            Length = length;
        }
    }
}
