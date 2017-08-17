namespace Vcf.Sample
{
    internal static class FailedFilter
    {
        /// <summary>
        /// returns the failed filter flag
        /// </summary>
        public static bool GetFailedFilter(IntermediateSampleFields intermediateSampleFields)
        {
            if (intermediateSampleFields.FormatIndices.FT == null) return false;
            var filterValue = intermediateSampleFields.SampleColumns[intermediateSampleFields.FormatIndices.FT.Value];
            return filterValue != "PASS" && filterValue != ".";
        }
    }
}
