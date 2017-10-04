using System;

namespace Vcf
{
    public static class StringExtensions
    {
        public delegate bool TryParse<T>(string str, out T value);

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

        public static T[] SplitToArray<T>(this string str, char separator, TryParse<T> parseFunc)
        {
            try
            {
                var contents = str.Split(separator);
                var vals = new T[contents.Length];

                for (int i = 0; i < contents.Length; i++)
                {
                    if (!parseFunc(contents[i], out T val)) return null;
                    vals[i] = val;
                }

                return vals;
            }
            catch (InvalidCastException)
            {
                return null;
            }
        }

        

    }
}