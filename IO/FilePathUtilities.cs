using System;

namespace IO
{
    public static class StringExtensions
    {
        public static string TrimStartToLast(this string s, string value, bool includeSeparator = false)
        {
            int extPos = s.LastIndexOf(value, StringComparison.Ordinal);
            if (extPos == -1) return s;
            return includeSeparator ? s.Substring(extPos) : s.Substring(extPos + value.Length);
        }

        public static string TrimEndFromFirst(this string s, string value, bool includeSeparator = false)
        {
            int extPos = s.IndexOf(value, StringComparison.Ordinal);
            if (extPos == -1) return s;
            return includeSeparator ? s.Substring(0, extPos + value.Length) : s.Substring(0, extPos);
        }

        public static string GetFileSuffix(this string s, bool includeDot) => ConnectUtilities.IsHttpLocation(s) ? s.TrimEndFromFirst("?").TrimStartToLast(".", includeDot) : s.TrimStartToLast(".", includeDot);
    }
}