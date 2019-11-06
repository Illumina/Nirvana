using OptimizedCore;

namespace Vcf.Sample.Legacy
{
    internal static class GenotypeQuality
    {
        public static int? GetGenotypeQuality(IntermediateSampleFields intermediateSampleFields)
        {
            bool hasGqx = intermediateSampleFields.FormatIndices.GQX != null;
            bool hasGq  = intermediateSampleFields.FormatIndices.GQ != null;

            if (!hasGqx && !hasGq)  return null;

            int gqIndex = hasGqx ? intermediateSampleFields.FormatIndices.GQX.Value : intermediateSampleFields.FormatIndices.GQ.Value;
            if (intermediateSampleFields.SampleColumns.Length <= gqIndex) return null;

            string gq = intermediateSampleFields.SampleColumns[gqIndex];

            (int number, bool foundError) = gq.OptimizedParseInt32();
            return foundError ? null : (int?)number;
        }
    }
}
