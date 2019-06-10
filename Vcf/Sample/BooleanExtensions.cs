namespace Vcf.Sample
{
    internal static class BooleanExtensions
    {
        internal static bool GetFailedFilter(this string ftField)
        {
            if (string.IsNullOrEmpty(ftField)) return false;
            return ftField != "PASS" && ftField != ".";
        }

        internal static bool IsDeNovo(this string dnField)
        {
            if (string.IsNullOrEmpty(dnField)) return false;
            return dnField == "DeNovo";
        }
    }
}
