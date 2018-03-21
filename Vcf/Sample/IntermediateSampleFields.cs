using VariantAnnotation.Interface.IO;

namespace Vcf.Sample
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
        public string RepeatNumber { get; }
        public string RepeatNumberSpan { get; }
        public float? DenovoQuality { get; }
        public float? AQ { get; }
        public float? LQ { get; }
        public double? VF { get; }

        // ReSharper disable InconsistentNaming
        public int? TIR { get; }
        public int? TAR { get; }
        public int? ACount { get; }
        public int? CCount { get; }
        public int? GCount { get; }
        public int? TCount { get; }
        public int? NR { get; }
        public int? NV { get; }
        public int[] MAD { get; }
        public string SCH { get; }
        public int[] PLG { get; }
        public int[] PCN { get; }
        public string[] DCS { get; }
        public string[] DID { get; }
        public string[] DST { get; }
        public int[] PCH { get; }
        public bool CHC { get; }
        // ReSharper restore InconsistentNaming

        public IntermediateSampleFields(string[] vcfColumns, FormatIndices formatIndices, string[] sampleCols)
        {
            VcfRefAllele  = vcfColumns[VcfCommon.RefIndex];
            AltAlleles    = vcfColumns[VcfCommon.AltIndex].Split(',');
            FormatIndices = formatIndices;
            SampleColumns = sampleCols;

            (TAR, TIR)           = GetLinkedIntegers(GetFirstValue(GetString(formatIndices.TAR, sampleCols)), GetFirstValue(GetString(formatIndices.TIR, sampleCols)));
            (NR, NV)             = GetLinkedIntegers(GetString(formatIndices.NR, sampleCols), GetString(formatIndices.NV, sampleCols));
            RepeatNumberSpan     = GetString(formatIndices.CI, sampleCols);
            MajorChromosomeCount = GetInteger(GetString(formatIndices.MCC, sampleCols));
            DenovoQuality        = GetFloat(GetString(formatIndices.DQ, sampleCols));
            MAD                  = GetIntegers(GetString(formatIndices.MAD, sampleCols));
            SCH                  = GetString(formatIndices.SCH, sampleCols);
            PLG                  = GetIntegers(GetString(formatIndices.PLG, sampleCols));
            PCN                  = GetIntegers(GetString(formatIndices.PCN, sampleCols));
            DCS                  = GetStrings(GetString(formatIndices.DCS, sampleCols));
            DID                  = GetStrings(GetString(formatIndices.DID, sampleCols));
            DST                  = GetStrings(GetString(formatIndices.DST, sampleCols));
            PCH                  = GetIntegers(GetString(formatIndices.PCH, sampleCols));
            CHC                  = GetBool(GetString(formatIndices.CHC, sampleCols), "+");
            AQ                   = GetFloat(GetString(formatIndices.AQ, sampleCols));
            LQ                   = GetFloat(GetString(formatIndices.LQ, sampleCols));
            VF                   = GetDouble(GetString(formatIndices.VF, sampleCols));

            (CopyNumber, RepeatNumber) = GetCopyNumber(GetString(formatIndices.CN, sampleCols), vcfColumns[VcfCommon.AltIndex].Contains("STR"));

            (ACount, CCount, GCount, TCount, TotalAlleleCount) = GetAlleleCounts(
                GetString(formatIndices.AU, sampleCols), GetString(formatIndices.CU, sampleCols),
                GetString(formatIndices.GU, sampleCols), GetString(formatIndices.TU, sampleCols));
        }

        private static string GetString(int? index, string[] cols)
        {
            if (index == null) return null;
            var s = cols[index.Value];
            return s == "." ? null : s;
        }

        internal static bool GetBool(string s, string trueString)
        {
            if (s == null) return false;
            return s == trueString;
        }

        internal static float? GetFloat(string s)
        {
            if (s == null) return null;
            if (float.TryParse(s, out var ret)) return ret;
            return null;
        }

        internal static double? GetDouble(string s)
        {
            if (s == null) return null;
            if (double.TryParse(s, out var ret)) return ret;
            return null;
        }

        internal static int? GetInteger(string s)
        {
            if (s == null) return null;
            if (int.TryParse(s, out var ret)) return ret;
            return null;
        }

        private static int[] GetIntegers(string s)
        {
            var cols = GetStrings(s);
            if (cols == null) return null;

            var result = new int[cols.Length];
            for (int i = 0; i < cols.Length; i++) result[i] = int.Parse(cols[i]);
            return result;
        }

        private static (int?, int?) GetLinkedIntegers(string s, string s2)
        {
            var num = GetInteger(s);
            var num2 = GetInteger(s2);
            if (num == null || num2 == null) return (null, null);
            return (num, num2);
        }

        private static string[] GetStrings(string s) => s?.Split(',');

        private static (int? CopyNumber, string RepeatNumber) GetCopyNumber(string s, bool containsStr)
        {
            if (s == null) return (null, null);
            if (containsStr) return (null, s);
            return (GetInteger(s), null);
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