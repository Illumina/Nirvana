using System;
using System.Text;

namespace CommonUtilities
{
    /// <summary>
    /// The StringBuilderCache reduces the number of StringBuilders created during the NA12878 VCF by 5.3x (20.2M to 3.8M)
    /// </summary>
    public static class StringBuilderCache
    {
        // 99% of all StringBuilders in a NA12878 VCF have length 3130 or less
        private const int DefaultCapacity = 3130;

        [ThreadStatic]
        private static StringBuilder _cachedInstance;

        public static StringBuilder Acquire(int capacity = DefaultCapacity)
        {
            StringBuilder sb = _cachedInstance;
            if (sb == null) return new StringBuilder(capacity);

            _cachedInstance = null;
            sb.Clear();
            return sb;
        }

        public static string GetStringAndRelease(StringBuilder sb)
        {
            string result = sb.ToString();
            _cachedInstance = sb;
            return result;
        }
    }
}