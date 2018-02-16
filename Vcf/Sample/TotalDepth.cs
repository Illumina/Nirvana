namespace Vcf.Sample
{
    internal static class TotalDepth
    {
        /// <summary>
        /// returns the total depth given different sources of information
        /// </summary>
        public static int? GetTotalDepth(int? infoDepth, IntermediateSampleFields intermediateSampleFields)
        {
            // use TAR & TIR
            if (intermediateSampleFields.TAR != null && intermediateSampleFields.TIR != null) return GetTotalDepthUsingTarTir(intermediateSampleFields);

            // use base counts
            if (intermediateSampleFields.TotalAlleleCount != null) return GetTotalDepthUsingAlleleCounts(intermediateSampleFields);

            // use DPI
            if (intermediateSampleFields.FormatIndices.DPI != null) return GetTotalDepthUsingDpi(intermediateSampleFields);

            // use DP
            if (intermediateSampleFields.FormatIndices.DP != null) return GetTotalDepthUsingDp(intermediateSampleFields);

            // use NR
            if (intermediateSampleFields.NR != null && intermediateSampleFields.AltAlleles.Length == 1) return intermediateSampleFields.NR;

            // use INFO DP (Pisces)
            return infoDepth;
        }

        /// <summary>
        /// returns the total depth using TAR & TIR
        /// </summary>
        private static int? GetTotalDepthUsingTarTir(IntermediateSampleFields intermediateSampleFields)
        {
            return intermediateSampleFields.TAR + intermediateSampleFields.TIR;
        }

        /// <summary>
        /// returns the total depth using tier 1 allele counts
        /// </summary>
        private static int? GetTotalDepthUsingAlleleCounts(IntermediateSampleFields intermediateSampleFields)
        {
            return intermediateSampleFields.TotalAlleleCount;
        }

        /// <summary>
        /// returns the total depth using DPI
        /// </summary>
        private static int? GetTotalDepthUsingDpi(IntermediateSampleFields intermediateSampleFields)
        {
            if (intermediateSampleFields.FormatIndices.DPI == null || intermediateSampleFields.SampleColumns.Length <= intermediateSampleFields.FormatIndices.DPI.Value) return null;
            var depth = intermediateSampleFields.SampleColumns[intermediateSampleFields.FormatIndices.DPI.Value];
            if (int.TryParse(depth, out int num)) return num;
            return null;

        }

        /// <summary>
        /// returns the total depth using DP
        /// </summary>
        private static int? GetTotalDepthUsingDp(IntermediateSampleFields intermediateSampleFields)
        {
            if (intermediateSampleFields.FormatIndices.DP == null || intermediateSampleFields.SampleColumns.Length <= intermediateSampleFields.FormatIndices.DP.Value) return null;
            var depth = intermediateSampleFields.SampleColumns[intermediateSampleFields.FormatIndices.DP.Value];
            if (int.TryParse(depth, out int num)) return num;
            return null;
        }
    }
}
