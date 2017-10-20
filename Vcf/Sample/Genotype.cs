namespace Vcf.Sample
{
    internal static class Genotype
    {
        /// <summary>
        /// returns the genotype flag
        /// </summary>
        public static string GetGenotype(IntermediateSampleFields intermediateSampleFields)
        {
            if (intermediateSampleFields.FormatIndices.GT == null) return null;
            var genotype = intermediateSampleFields.SampleColumns[intermediateSampleFields.FormatIndices.GT.Value];
            return genotype == "." ? null : genotype;
        }

    }
}
