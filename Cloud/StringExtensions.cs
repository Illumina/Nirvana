using System;

namespace Cloud
{
    public static class StringExtensions
    {
        public static string TrimStartToLast(this string s, string value)
        {
            int extPos = s.LastIndexOf(value, StringComparison.Ordinal);
            return extPos == -1 ? s : s.Substring(extPos + value.Length);
        }

        public static string TrimEndFromFirst(this string s, string value)
        {
            int extPos = s.IndexOf(value, StringComparison.Ordinal);
            return extPos == -1 ? s : s.Substring(0, extPos);
        }
    }
}