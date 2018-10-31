using System.IO;
using IO.StreamSource;
using IO.StreamSourceCollection;

namespace IO
{
    public static class StreamSourceUtils
    {
        public static Stream GetStream(string location) => GetStreamSource(location).GetStream();

        public static IStreamSource GetStreamSource(string location)
        {
            if (IsWebLocation(location)) return new HttpStreamSource(location);
            return File.Exists(location) ? new FileStreamSource(location) : null;
        }

        public static IStreamSourceCollection GetStreamSourceCollection(string location)
        {
            if (location == null) return null;
            if (IsWebLocation(location)) return new HttpStreamSourceCollection(location);
            return Directory.Exists(location) ? new FileDirectory(location) : null;
        }

        private static bool IsWebLocation(string path) => path.ToLower().StartsWith("http");

    }
}