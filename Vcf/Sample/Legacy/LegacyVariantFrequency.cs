using OptimizedCore;

namespace Vcf.Sample.Legacy
{
    internal static class LegacyVariantFrequency
    {
        private static readonly double[] ZeroVf = { 0.0 };

        public static double[] GetVariantFrequencies(IntermediateSampleFields sampleFields)
        {
            double[] vf = null;

            // use VF
            if (sampleFields.VF != null) vf = GetVariantFrequenciesUsingVf(sampleFields);

            // use TAR & TIR
            if (sampleFields.TAR != null && sampleFields.TIR != null) vf = GetVariantFrequenciesUsingTarTir(sampleFields);

            // use allele counts
            if (vf == null && sampleFields.TotalAlleleCount != null) vf = GetVariantFrequenciesUsingAlleleCounts(sampleFields);

            // use allele depths
            if (vf == null && sampleFields.FormatIndices.AD != null) vf = GetVariantFrequenciesUsingAlleleDepths(sampleFields);

            

            return vf;
        }

        private static double[] GetVariantFrequenciesUsingVf(IntermediateSampleFields sampleFields)
        {
            if (sampleFields.AltAlleles.Length > 1 || sampleFields.VF == null) return null;
            return new[] { sampleFields.VF.Value };
        }

        private static double[] GetVariantFrequenciesUsingAlleleCounts(IntermediateSampleFields sampleFields)
        {
            bool isRefSingleBase      = sampleFields.VcfRefAllele.Length == 1;
            bool areAllAltsSingleBase = sampleFields.AltAlleles.AreAllAltAllelesSingleBase();
            bool isReference          = sampleFields.AltAlleles.Length == 1 && sampleFields.AltAlleles[0] == ".";

            // for this to work we need a single-base reference allele and all raw allele counts must be available
            if (sampleFields.TotalAlleleCount == null || isReference || !isRefSingleBase || !areAllAltsSingleBase) return null;

            int numAltAlleles = sampleFields.AltAlleles.Length;
            var variantFreqs  = new double[numAltAlleles];

            if (sampleFields.TotalAlleleCount == 0) return variantFreqs;

            for (var i = 0; i < numAltAlleles; i++)
            {
                int alleleCount = GetAlleleCount(sampleFields, i);
                variantFreqs[i] = alleleCount / (double)sampleFields.TotalAlleleCount;
            }

            return variantFreqs;
        }

        internal static bool AreAllAltAllelesSingleBase(this string[] altAlleles)
        {
            foreach (string altAllele in altAlleles)
                if (altAllele.Length != 1)
                    return false;
            return true;
        }

        private static int GetAlleleCount(IntermediateSampleFields sampleFields, int alleleIndex)
        {
            string altAllele = sampleFields.AltAlleles[alleleIndex];
            var alleleCount = 0;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (altAllele)
            {
                case "A":
                    alleleCount = sampleFields.ACount ?? 0;
                    break;
                case "C":
                    alleleCount = sampleFields.CCount ?? 0;
                    break;
                case "G":
                    alleleCount = sampleFields.GCount ?? 0;
                    break;
                case "T":
                    alleleCount = sampleFields.TCount ?? 0;
                    break;
            }

            return alleleCount;
        }

        private static double[] GetVariantFrequenciesUsingTarTir(IntermediateSampleFields sampleFields)
        {
            // TAR and TIR: never observed with multiple alternate alleles
            if (sampleFields.TIR == null || sampleFields.TAR == null || sampleFields.AltAlleles.Length > 1) return null;
            if (sampleFields.TIR + sampleFields.TAR == 0) return ZeroVf;

            var tir = (double)sampleFields.TIR;
            var tar = (double)sampleFields.TAR;
            return new[] { tir / (tar + tir) };
        }

        private static double[] GetVariantFrequenciesUsingAlleleDepths(IntermediateSampleFields sampleFields)
        {
            if (sampleFields.FormatIndices.AD == null || sampleFields.SampleColumns.Length <= sampleFields.FormatIndices.AD.Value) return null;

            int numAltAlleles = sampleFields.AltAlleles.Length;
            var variantFreqs  = new double[numAltAlleles];

            string adField = sampleFields.SampleColumns[sampleFields.FormatIndices.AD.Value];
            (var alleleDepths, bool allValuesAreValid, int totalDepth) = GetAlleleDepths(adField);
            if (!allValuesAreValid || numAltAlleles != alleleDepths.Length) return null;

            // sanity check: make sure we handle NaNs properly
            if (totalDepth == 0) return variantFreqs;

            for (var alleleIndex = 0; alleleIndex < numAltAlleles; alleleIndex++)
            {
                variantFreqs[alleleIndex] = alleleDepths[alleleIndex] / (double)totalDepth;
            }

            return variantFreqs;
        }

        private static (int[] AlleleDepths, bool AllValuesAreValid, int totalDepth) GetAlleleDepths(string adField)
        {
            var adFields = adField.OptimizedSplit(',');
            var alleleDepths = new int[adFields.Length - 1];
            var totalDepth = 0;

            for (var i = 0; i < adFields.Length; i++)
            {
                (int ad, bool foundError) = adFields[i].OptimizedParseInt32();
                if(foundError) return (null, false, totalDepth);
                if (i > 0) alleleDepths[i - 1] = ad;
                totalDepth += ad;
            }

            return (alleleDepths, true, totalDepth);
        }
    }
}
