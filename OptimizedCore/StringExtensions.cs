using System;

namespace OptimizedCore
{
    public static class StringExtensions
    {
        public static unsafe string[] OptimizedSplit(this string s, char delimiter, int numColumns = -1)
        {
            var numReplaces = 0;
            int sLen        = s.Length;
            var sepList     = new int[s.Length];

            // find the locations of our tab delimiter
            fixed (char* chPtr = s)
            {
                for (var index = 0; index < sLen; ++index)
                {
                    if (chPtr[index] == delimiter) sepList[numReplaces++] = index;
                }
            }

            // extract our columns
            var startIndex = 0;
            var colIndex   = 0;

            int numDelimitedColumns = numReplaces + 1;
            if (numColumns < numDelimitedColumns) numColumns = numDelimitedColumns;

            var columns = new string[numColumns];
            for (var index = 0; index < numReplaces && startIndex < sLen; ++index)
            {
                columns[colIndex++] = s.Substring(startIndex, sepList[index] - startIndex);
                startIndex = sepList[index] + 1;
            }

            // handle the last column
            if (startIndex < sLen && numReplaces >= 0) columns[colIndex] = s.Substring(startIndex);
            else if (colIndex == numReplaces) columns[colIndex] = string.Empty;

            return columns;
        }

        public static (string Key, string Value) OptimizedKeyValue(this string s)
        {
            int equalPos = s.IndexOf('=');
            return equalPos == -1 ? (s, null) : (s.Substring(0, equalPos), s.Substring(equalPos + 1));
        }

        /// <summary>
        /// handles -2_147_483_647 to +2_147_483_647
        /// </summary>
        public static unsafe (int Number, bool FoundError) OptimizedParseInt32(this string s)
        {
            var number = 0;

            // 2_147_483_647
            if (string.IsNullOrEmpty(s) || s.Length > 11) return (0, true);

            try
            {
                fixed (char* chPtr = s)
                {
                    int index         = s.Length - 1;
                    var ptr           = chPtr;
                    var applyNegative = false;

                    if (*ptr == '-')
                    {
                        applyNegative = true;
                        ptr++;
                        index--;
                    }

                    while (index >= 0)
                    {
                        if (*ptr < 48 || *ptr > 57) return (0, true);

                        checked
                        {
                            number *= 10;
                            number += *ptr++ - '0';
                        }

                        index--;
                    }

                    if (applyNegative) number = -number;
                }
            }
            catch (OverflowException)
            {
                return (0, true);
            }

            return (number, false);
        }

        public static bool OptimizedStartsWith(this string s, char ch) => s.Length > 0 && s[0] == ch;

        public static bool OptimizedEndsWith(this string s, char ch) => s.Length > 0 && s[s.Length - 1] == ch;
    }
}
