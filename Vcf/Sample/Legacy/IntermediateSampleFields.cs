using OptimizedCore;
using VariantAnnotation.Interface.IO;

namespace Vcf.Sample.Legacy
{
    public sealed class IntermediateSampleFields
    {
        public FormatIndices FormatIndices { get; }
        public string[] SampleColumns { get; }
        public string[] AltAlleles { get; }

        public int? TotalAlleleCount { get; }
        public string VcfRefAllele { get; }
        public int? MajorChromosomeCount { get; }
        public int? CopyNumber { get; }

        // ReSharper disable InconsistentNaming
        public float? AQ { get; }
        public float? LQ { get; }
        public double? VF { get; }
        public int? TIR { get; }
        public int? TAR { get; }
        public int? ACount { get; }
        public int? CCount { get; }
        public int? GCount { get; }
        public int? TCount { get; }

        public string[] DST { get; }
        // ReSharper restore InconsistentNaming

        // ReSharper disable once SuggestBaseTypeForParameter
        public IntermediateSampleFields(string[] vcfColumns, FormatIndices formatIndices, string[] sampleCols)
        {
            VcfRefAllele  = vcfColumns[VcfCommon.RefIndex];
            AltAlleles    = vcfColumns[VcfCommon.AltIndex].OptimizedSplit(',');
            FormatIndices = formatIndices;
            SampleColumns = sampleCols;

            (TAR, TIR)           = GetLinkedIntegers(GetFirstValue(GetString(formatIndices.TAR, sampleCols)), GetFirstValue(GetString(formatIndices.TIR, sampleCols)));
            MajorChromosomeCount = GetInteger(GetString(formatIndices.MCC, sampleCols));
            DST                  = GetStrings(GetString(formatIndices.DST, sampleCols));
            AQ                   = GetFloat(GetString(formatIndices.AQ, sampleCols));
            LQ                   = GetFloat(GetString(formatIndices.LQ, sampleCols));
            VF                   = GetDouble(GetString(formatIndices.VF, sampleCols));

            CopyNumber = GetCopyNumber(GetString(formatIndices.CN, sampleCols), vcfColumns[VcfCommon.AltIndex].Contains("STR"));

            (ACount, CCount, GCount, TCount, TotalAlleleCount) = GetAlleleCounts(
                GetString(formatIndices.AU, sampleCols), GetString(formatIndices.CU, sampleCols),
                GetString(formatIndices.GU, sampleCols), GetString(formatIndices.TU, sampleCols));
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static string GetString(int? index, string[] cols)
        {
            if (index == null || index >= cols.Length) return null;
            string s = cols[index.Value];
            return s == "." ? null : s;
        }

        private static float? GetFloat(string s)
        {
            if (s == null) return null;
            if (float.TryParse(s, out float ret)) return ret;
            return null;
        }

        private static double? GetDouble(string s)
        {
            if (s == null) return null;
            if (double.TryParse(s, out double ret)) return ret;
            return null;
        }

        private static int? GetInteger(string s)
        {
            if (s == null) return null;
            (int number, bool foundError) = s.OptimizedParseInt32();
            return foundError ? null : (int?)number;
        }

        private static (int?, int?) GetLinkedIntegers(string s, string s2)
        {
            var num = GetInteger(s);
            var num2 = GetInteger(s2);
            if (num == null || num2 == null) return (null, null);
            return (num, num2);
        }

        private static string[] GetStrings(string s) => s?.OptimizedSplit(',');

        private static int? GetCopyNumber(string s, bool containsStr)
        {
            if (s == null || containsStr) return null;
            return GetInteger(s);
        }

        private static (int?, int?, int?, int?, int?) GetAlleleCounts(string au, string cu, string gu, string tu)
        {
            if (au == null || cu == null || gu == null || tu == null) return (null, null, null, null, null);

            var a = GetInteger(GetFirstValue(au));
            var c = GetInteger(GetFirstValue(cu));
            var g = GetInteger(GetFirstValue(gu));
            var t = GetInteger(GetFirstValue(tu));
            var total = a == null || c == null || g == null || t == null ? null : a + c + g + t;
            return (a, c, g, t, total);
        }

        private static string GetFirstValue(string s) => GetStrings(s)?[0];
    }
}