using OptimizedCore;

namespace Vcf.Sample
{
    public static class SampleParsingExtensions
    {
        internal static string GetString(this string[] cols, int? index) => index == null ? null : cols[index.Value];

        internal static float? GetFloat(this string s)
        {
            if (s == null) return null;
            if (float.TryParse(s, out float num)) return num;
            return null;
        }

        internal static double? GetDouble(this string s)
        {
            if (s == null) return null;
            if (double.TryParse(s, out double num)) return num;
            return null;
        }

        internal static int? GetInteger(this string s)
        {
            if (s == null) return null;
            (int number, bool foundError) = s.OptimizedParseInt32();
            return foundError ? null : (int?)number;
        }

        internal static string[] GetStrings(this string s) => s?.OptimizedSplit(',');

        public static int[] GetIntegers(this string s, char delimiter = ',')
        {
            if (s == null) return null;

            var cols   = s.OptimizedSplit(delimiter);
            var values = new int[cols.Length];

            for (var i = 0; i < values.Length; i++)
            {
                (int number, bool foundError) = cols[i].OptimizedParseInt32();
                if (foundError) return null;
                values[i] = number;
            }

            return values;
        }
    }
}