using System;
using System.IO;
using System.Threading;

namespace IO
{
    public sealed class PersistentStream : Stream
    {
        private Stream _stream;
        private readonly Func<long, Stream> _connectFunc;
        private long _reconnectPosition;
        
        private const int MaxRetryAttempts     = 5;
        private const int NumRetryMilliseconds = 2_000;

        #region Stream

        public override bool CanRead  => _stream.CanRead;
        public override bool CanSeek => _stream.CanSeek;
        public override bool CanWrite => _stream.CanWrite;
        public override long Length   => _stream.Length;

        public override long Position
        {
            get => _reconnectPosition;
            set => SetPosition(value);
        }

        public override void Flush()                                     => _stream.Flush();
        public override long Seek(long offset, SeekOrigin origin)        => _stream.Seek(offset, origin);
        public override void SetLength(long value)                       => _stream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);

        #endregion

        public PersistentStream(Stream stream, Func<long, Stream> connectFunc, long position)
        {
            _stream            = stream;
            _connectFunc       = connectFunc;
            _reconnectPosition = position;
        }

        [Obsolete("should be removed ASAP")]
        private void SetPosition(long position)
        {
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));
            _stream?.Dispose();

            _stream = ConnectUtilities.ConnectWithRetries(_connectFunc, position, MaxRetryAttempts);
            _reconnectPosition = position;
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
                count        -= cnt;
            }

            _reconnectPosition += numBytesRead;
            return numBytesRead;
        }

        private int PersistentRead(byte[] buffer, int offset, int count)
        {
            bool keepTrying  = true;
            int numRetries   = 0;
            int numBytesRead = 0;

            while (keepTrying)
            {
                try
                {
                    numBytesRead = _stream.Read(buffer, offset, count); 
                    keepTrying = false;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"EXCEPTION: {e.Message}");
                    if (numRetries == MaxRetryAttempts) throw;

                    Thread.Sleep(NumRetryMilliseconds);
                    _stream = ConnectUtilities.ConnectWithRetries(_connectFunc, _reconnectPosition, MaxRetryAttempts-numRetries);

                    numRetries++;                    
                }
            }

            return numBytesRead;
        }
    }
}