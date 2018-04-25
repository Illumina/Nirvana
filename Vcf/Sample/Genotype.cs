namespace Vcf.Sample
{
    internal static class Genotype
    {
        public static string GetGenotype(IntermediateSampleFields intermediateSampleFields)
        {
            if (intermediateSampleFields.FormatIndices.GT == null) return null;
            string genotype = intermediateSampleFields.SampleColumns[intermediateSampleFields.FormatIndices.GT.Value];
            return genotype == "." ? null : genotype;
        }
    }
}
