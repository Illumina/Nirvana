using System.IO;
using System.Net;

namespace IO
{
    public sealed class HttpFileStream:Stream
    {
        private readonly string _url;
        public static int Count;

        public HttpFileStream(string url)
        {
            _url = url;
            Position = 0;
        }

        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            HttpWebRequest myReq = (HttpWebRequest) WebRequest.Create(_url);
            Count++;
            myReq.AddRange(Position);
            HttpWebResponse response = (HttpWebResponse)myReq.GetResponse();

            int byteCount=0;
            using (var stream = response.GetResponseStream())
            {
                byteCount += stream.ForcedRead(buffer, offset, count);
            }

            return byteCount;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
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

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => -1;

        public override long Position {get;set;}
    }
}