namespace Vcf.Sample
{
    internal static class GenotypeQuality
    {
        /// <summary>
        /// returns the genotype quality given different sources of information
        /// </summary>
        public static int? GetGenotypeQuality(IntermediateSampleFields intermediateSampleFields)
        {
            var hasGqx = intermediateSampleFields.FormatIndices.GQX != null;
            var hasGq  = intermediateSampleFields.FormatIndices.GQ != null;

            if (!hasGqx && !hasGq)  return null;

            var gqIndex = hasGqx ? intermediateSampleFields.FormatIndices.GQX.Value : intermediateSampleFields.FormatIndices.GQ.Value;
            if (intermediateSampleFields.SampleColumns.Length <= gqIndex) return null;

            var gq = intermediateSampleFields.SampleColumns[gqIndex];
            if (int.TryParse(gq, out int num)) return num;
            return null;
        }
    }
}
