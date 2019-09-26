using System;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;

namespace IO
{
    public sealed class PersistentStream : Stream
    {
        private readonly string _url;
        private HttpWebResponse _response;
        private Stream _stream;
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
            set
            {
                Disconnect();
                Connect(value);
                _position = value;
            }
        }

        public PersistentStream(string url, long position)
        {
            _url      = url;
            _position = position;

            Connect(position);
        }

        private void Connect(long position)
        {
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));

            var request = WebRequest.CreateHttp(_url);
            request.AddRange(position);
            _response = (HttpWebResponse)request.GetResponse();
            _stream   = _response.GetResponseStream();
        }

        private void Disconnect()
        {
            _response?.Dispose();
            _stream?.Dispose();
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
                    Log(MethodName(), e);
                    if (numRetries == MaxRetryAttempts) throw;

                    Disconnect();
                    Thread.Sleep(NumRetryMilliseconds);
                    Connect(_position);
                    numRetries++;                    
                }
            }

            return numBytesRead;
        }

        private static void Log(string methodName, Exception e) =>
            Logger.LogLine($"{methodName}: {e.GetType()}: {e.Message}");

        private static string MethodName([CallerMemberName] string caller = null) => caller;

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing) Disconnect();

                _response = null;
                _stream   = null;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}