namespace Vcf.Sample
{
    internal static class VariantFrequency
    {
        public static double[] GetVariantFrequencies(double? vfField, int[] alleleDepths, int numAltAlleles)
        {
            // use VF
            double[] vf = GetVariantFrequenciesUsingVf(vfField, numAltAlleles > 1);

            // use allele depths
            if (vf == null) vf = GetVariantFrequenciesUsingAlleleDepths(alleleDepths, numAltAlleles);

            return vf;
        }

        private static double[] GetVariantFrequenciesUsingVf(double? vf, bool multipleAltAlleles)
        {
            if (multipleAltAlleles || vf == null) return null;
            return new[] { vf.Value };
        }

        private static double[] GetVariantFrequenciesUsingAlleleDepths(int[] alleleDepths, int numAltAlleles)
        {
            if (alleleDepths == null) return null;
            if (numAltAlleles + 1 != alleleDepths.Length) return null;

            var variantFreqs = new double[numAltAlleles];

            int totalDepth = 0;
            for (var alleleIndex = 0; alleleIndex < alleleDepths.Length; alleleIndex++)
                totalDepth += alleleDepths[alleleIndex];

            if (totalDepth == 0) return variantFreqs;

            for (var alleleIndex = 0; alleleIndex < numAltAlleles; alleleIndex++)
            {
                variantFreqs[alleleIndex] = alleleDepths[alleleIndex + 1] / (double)totalDepth;
            }

            return variantFreqs;
        }
    }
}
