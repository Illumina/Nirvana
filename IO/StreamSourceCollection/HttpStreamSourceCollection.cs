using System.Collections.Generic;
using System.IO;
using IO.StreamSource;

namespace IO.StreamSourceCollection
{
    public sealed class HttpStreamSourceCollection : IStreamSourceCollection
    {
        private readonly string _url;

        public HttpStreamSourceCollection(string url)
        {
            _url = url;
        }

        public IEnumerable<IStreamSource> GetStreamSources() => GetStreamSources("");

        public IEnumerable<IStreamSource> GetStreamSources(string suffix)
        {
            var streamSources = new List<IStreamSource>();
            var reader = new StreamReader(((IStreamSource) new HttpStreamSource(_url)).GetStream());
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.TrimEnd();
                if (!line.EndsWith(suffix)) continue;
                streamSources.Add(new HttpStreamSource(line));
            }

            return streamSources;
        }
    }
}