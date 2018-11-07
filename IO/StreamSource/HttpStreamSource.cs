using System;
using System.IO;
using System.Net;

namespace IO.StreamSource
{
    public sealed class HttpStreamSource : IStreamSource
    {
        private readonly string _url;
        public static int Count;
        public HttpStreamSource(string url)
        {
            _url = url;
        }

        public Stream GetStream(long start) => new SeekableStream(this, start);

        public Stream GetRawStream(long start, long end)
        {
            Count++;
            var stream = FailureRecovery.CallWithRetry(() => TryGetRawStream(start, end), out int retryCounter, 8);
            if (retryCounter > 0) Console.WriteLine($"Retried {retryCounter} time(s) in GetRawStream method.");
            return stream;
        }

        private Stream TryGetRawStream(long start, long end)
        {
            var request = WebRequest.CreateHttp(_url);
            if (start >= 0) request.AddRange(start, end);

            request.Timeout = 10_000;
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