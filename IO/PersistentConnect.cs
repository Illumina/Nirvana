using System;
using System.IO;
using System.Net;

namespace IO
{
    public sealed class PersistentConnect : IConnect
    {
        private readonly string _url;

        public PersistentConnect(string url) => _url = url;

        public (HttpWebResponse Response, Stream Stream) Connect(long position)
        {
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));
            var request = WebRequest.CreateHttp(_url);
            request.AddRange(position);
            var response = (HttpWebResponse)request.GetResponse();
            var stream = response.GetResponseStream();
            return (response, stream);
        }
    }
}