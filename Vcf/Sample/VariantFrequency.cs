﻿namespace Vcf.Sample
{
    internal static class VariantFrequency
    {
        public static double[] GetVariantFrequencies(double? vfField, int[] alleleDepths, int numAltAlleles)
        {
            // use VF
            double[] vf = GetVariantFrequenciesUsingVf(vfField, numAltAlleles > 1) ?? GetVariantFrequenciesUsingAlleleDepths(alleleDepths, numAltAlleles);

            // use allele depths

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

            var totalDepth = 0;
            foreach (int ad in alleleDepths) totalDepth += ad;

            if (totalDepth == 0) return variantFreqs;

            for (var alleleIndex = 0; alleleIndex < numAltAlleles; alleleIndex++)
            {
                variantFreqs[alleleIndex] = alleleDepths[alleleIndex + 1] / (double)totalDepth;
            }

            return variantFreqs;
        }
    }
}
