using System;
using System.IO;
using System.Threading;

namespace IO
{
    public sealed class PersistentStream : Stream
    {
        private Stream _stream;
        private readonly Func<long, Stream> _connectFunc;
        private long _position;
        
        private const int MaxRetryAttempts     = 5;
        private const int NumRetryMilliseconds = 2_000;

        public override bool CanRead                                     => _stream.CanRead;
        public override bool CanSeek                                     => _stream.CanSeek;
        public override bool CanWrite                                    => _stream.CanWrite;
        public override long Length                                      => _stream.Length;
        public override void Flush()                                     => _stream.Flush();
        public override long Seek(long offset, SeekOrigin origin)        => _stream.Seek(offset, origin);
        public override void SetLength(long value)                       => _stream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);

        public override long Position
        {
            get => _position;
            set => SetPosition(value);
        }

        public PersistentStream(Stream stream, Func<long, Stream> connectFunc, long position)
        {
            _stream      = stream;
            _connectFunc = connectFunc;
            _position    = position;
        }

        //the final _stream needs to be disposed
        ~PersistentStream()
        {
            _stream?.Dispose();
        }

        [Obsolete("should be removed ASAP")]
        private void SetPosition(long position)
        {
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));
            _stream?.Dispose();

            _stream = ConnectUtilities.ConnectWithRetries(_connectFunc, position, MaxRetryAttempts);
            _position = position;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var numBytesRead = 0;
            
            while (count > 0)
            {
                int cnt = PersistentRead(buffer, offset, count);
                if (cnt == 0) return numBytesRead;

                offset       += cnt;
                numBytesRead += cnt;
                _position    += cnt;
                count        -= cnt;
            }

            return numBytesRead;
        }

        private int PersistentRead(byte[] buffer, int offset, int count)
        {
            var keepTrying   = true;
            var numRetries   = 0;
            var numBytesRead = 0;

            while (keepTrying)
            {
                try
                {
                    numBytesRead = _stream.Read(buffer, offset, count); 
                    keepTrying = false;
                }
                catch (Exception e)
                {
                    Logger.LogLine($"EXCEPTION: {e.Message}");
                    if (numRetries == MaxRetryAttempts) throw;

                    _stream?.Dispose();
                    Thread.Sleep(NumRetryMilliseconds);
                    _stream = ConnectUtilities.ConnectWithRetries(_connectFunc, _position, MaxRetryAttempts);

                    numRetries++;                    
                }
            }

            return numBytesRead;
        }
    }
}