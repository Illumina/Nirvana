using System;
using System.IO;

namespace IO
{
    public static class UrlUtilities
    {
        private const char UriSeparator = '/';

        public static string GetPath(string url)     => new Uri(url).LocalPath.TrimStart(UriSeparator);
        public static string GetFileName(string url) => Path.GetFileName(GetPath(url));

        public static string UrlCombine(this string prefix, string suffix) =>
            prefix.TrimEnd(UriSeparator) + UriSeparator + suffix.TrimStart(UriSeparator);
    }
}
