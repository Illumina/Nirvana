using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IO
{
    public static class PersistentStreamUtils
    {
        public static Stream GetReadStream(string urlOrPath, long position = 0)
        {
            if (string.IsNullOrEmpty(urlOrPath)) return null;

            if (!HttpUtilities.IsUrl(urlOrPath))
                return File.Exists(urlOrPath) ? FileUtilities.GetReadStream(urlOrPath) : null;

            return new PersistentStream(new PersistentConnect(urlOrPath), position);
        }

        public static List<Stream> GetStreams(List<string> locations)
        {
            if (locations == null) return null;

            var streams = new List<Stream>(locations.Count);
            streams.AddRange(locations.Select(urlOrPath => GetReadStream(urlOrPath)));
            return streams;
        }
    }
}