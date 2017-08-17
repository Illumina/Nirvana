using System;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public static class StringExtensions
    {

        public static int CommonPrefixLength(this string a, string b)
        {
            if (a == null || b == null) return 0;

            var maxPrefixLength = Math.Min(a.Length, b.Length);

            var prefixLength = 0;
            while (prefixLength < maxPrefixLength && a[prefixLength] == b[prefixLength]) prefixLength++;

            return prefixLength;
        }

        public static int CommonSuffixLength(this string a, string b)
        {
            if (a == null || b == null) return 0;

            var maxSuffixLength = Math.Min(a.Length, b.Length);

            var suffixLength = 0;
            while (suffixLength < maxSuffixLength &&
                   a[a.Length - suffixLength - 1] == b[b.Length - suffixLength - 1]) suffixLength++;

            return suffixLength;
        }
    }
}