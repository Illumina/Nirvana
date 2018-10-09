using System.IO;
using System.Net;

namespace IO.StreamSource
{
    public sealed class HttpStreamSource : IStreamSource
    {
        private readonly string _url;

        public HttpStreamSource(string url)
        {
            _url = url;
        }

        public Stream GetStream(long start) => new SeekableStream(this, start);

        public Stream GetRawStream(long start, long end)
        {
            var request = WebRequest.CreateHttp(_url);
            if (start >= 0) request.AddRange(start, end);
            return ((HttpWebResponse) request.GetResponse()).GetResponseStream();
        }


        public IStreamSource GetAssociatedStreamSource(string extraExtension) => new HttpStreamSource(_url + extraExtension);

        public long GetLength()
        {
            var request = (HttpWebRequest) WebRequest.Create(_url);
            request.Method = "HEAD";

            return ((HttpWebResponse) request.GetResponse()).ContentLength;
        }
    }
}