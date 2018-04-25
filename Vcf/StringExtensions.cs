using System;
using OptimizedCore;

namespace Vcf
{
    public static class StringExtensions
    {
        public delegate bool TryParse<T>(string str, out T value);

        public static int? GetNullableInt(this string str)
        {
            (int number, bool foundError) = str.OptimizedParseInt32();
            return foundError ? null : (int?) number;
        }

        public static T? GetNullableValue<T>(this string str, TryParse<T> parseFunc) where T : struct
        {
            try
            {
                if (parseFunc(str, out T val)) return val;
                return null;
            }
            catch (InvalidCastException)
            {
                return null;
            }
        }

        public static int[] SplitToArray(this string s)
        {
            try
            {
                var cols = s.OptimizedSplit(',');
                var values = new int[cols.Length];

                for (var i = 0; i < cols.Length; i++)
                {
                    (int number, bool foundError) = cols[i].OptimizedParseInt32();
                    if (foundError) return null;
                    values[i] = number;
                }

                return values;
            }
            catch (InvalidCastException)
            {
                return null;
            }
        }
    }
}