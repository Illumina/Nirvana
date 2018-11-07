using System;
using System.IO;
using IO.StreamSource;

namespace IO
{
    public sealed class SeekableStream : Stream
    {
        private readonly IStreamSource _streamSource;
        private Stream _stream;
        private long _position;
        private readonly long _end;
        private int _totalRetries;
        private const int MaxTries = 10;
        private bool _failureRecovery;

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

        private void InitiateStream(long start, long end)
        {
            if (_stream == null || _failureRecovery)
                _stream = _streamSource.GetRawStream(start, end);
        }

        public override void Flush()
        {
            _stream?.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readBytes = FailureRecovery.CallWithRetry(() => TryRead(buffer, offset, count), out int retryCounter);
            if (retryCounter > 0) Console.WriteLine($"Retried {retryCounter} time(s) in Read method in SeekableStream class.");

            _totalRetries += retryCounter;
            if (_totalRetries > MaxTries) throw new Exception($"Max number of retries (${MaxTries}) reached.");
            return readBytes;
        }

        private int TryRead(byte[] buffer, int offset, int count)
        {
            try
            {
                InitiateStream(_position, _end);

                int readCount = _stream.ForcedRead(buffer, offset, count);
                _position += readCount;

                _failureRecovery = false;

                return readCount;
            }
            catch (Exception)
            {
                _failureRecovery = true;
                throw;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            //todo
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public new void Dispose()
        {
            _stream?.Dispose();
        }
    }
}