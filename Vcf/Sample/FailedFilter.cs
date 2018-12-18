namespace Vcf.Sample
{
    internal static class FailedFilter
    {
        public static bool GetFailedFilter(IntermediateSampleFields intermediateSampleFields)
        {
            if (intermediateSampleFields.FormatIndices.FT == null) return false;
            if (intermediateSampleFields.FormatIndices.FT.Value >=
                intermediateSampleFields.SampleColumns.Length) return false;
            string filterValue = intermediateSampleFields.SampleColumns[intermediateSampleFields.FormatIndices.FT.Value];
            return filterValue != "PASS" && filterValue != ".";
        }
    }
}
