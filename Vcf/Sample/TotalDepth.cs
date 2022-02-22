using OptimizedCore;

namespace Vcf.Sample
{
    internal static class TotalDepth
    {
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

        private static int? GetTotalDepthUsingTarTir(IntermediateSampleFields intermediateSampleFields) => intermediateSampleFields.TAR + intermediateSampleFields.TIR;

        private static int? GetTotalDepthUsingAlleleCounts(IntermediateSampleFields intermediateSampleFields) => intermediateSampleFields.TotalAlleleCount;

        private static int? GetTotalDepthUsingDpi(IntermediateSampleFields intermediateSampleFields)
        {
            if (intermediateSampleFields.FormatIndices.DPI == null || intermediateSampleFields.SampleColumns.Length <= intermediateSampleFields.FormatIndices.DPI.Value) return null;
            string depth = intermediateSampleFields.SampleColumns[intermediateSampleFields.FormatIndices.DPI.Value];
            (int number, bool foundError) = depth.OptimizedParseInt32();
            return foundError ? null : number;
        }

        private static int? GetTotalDepthUsingDp(IntermediateSampleFields intermediateSampleFields)
        {
            if (intermediateSampleFields.FormatIndices.DP == null || intermediateSampleFields.SampleColumns.Length <= intermediateSampleFields.FormatIndices.DP.Value) return null;
            string depth = intermediateSampleFields.SampleColumns[intermediateSampleFields.FormatIndices.DP.Value];
            (int number, bool foundError) = depth.OptimizedParseInt32();
            return foundError ? null : number;
        }
    }
}
