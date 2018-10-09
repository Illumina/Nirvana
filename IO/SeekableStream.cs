using System.IO;
using IO.StreamSource;

namespace IO
{
    public sealed class SeekableStream : Stream
    {
        private readonly IStreamSource _streamSource;
        private Stream _stream;
        private long _position;
        private long _end;

        public override bool CanRead { get; }
        public override bool CanSeek { get; }
        public override bool CanWrite { get; }
        //Length is unknown by default
        public override long Length => _streamSource.GetLength();

        public override long Position
        {
            get => _position;
            set => SetPosition(value);
        }

        public SeekableStream(IStreamSource streamSource, long start, long end = long.MaxValue)
        {
            _streamSource = streamSource;
            _position = start;
            _end = end;
            CanRead = true;
            CanSeek = true;
            CanWrite = false;
        }

        private void SetPosition(long position) {
            _stream?.Dispose();
            _stream = _streamSource.GetRawStream(position, _end);
            _position = position;
        }

        private void TryInitiateStream(long start, long end)
        {
            if (_stream == null) _stream = _streamSource.GetRawStream(start, end);
        }

        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            TryInitiateStream(_position, _end);

            int readCount = _stream.ForcedRead(buffer, offset, count);
            _position += readCount;

            return readCount;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            //todo
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public new void Dispose()
        {
            _stream?.Dispose();
        }
    }
}