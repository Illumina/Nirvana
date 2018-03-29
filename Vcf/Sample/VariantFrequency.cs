using System.Linq;

namespace Vcf.Sample
{
    internal static class VariantFrequency
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

            // use NR & NV
            if (vf == null && sampleFields.NR != null && sampleFields.NV != null) vf = GetVariantFrequenciesUsingNrNv(sampleFields);

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
            bool areAllAltsSingleBase = sampleFields.AltAlleles.All(altAllele => altAllele.Length == 1);
            bool isReference          = sampleFields.AltAlleles.Length == 1 && sampleFields.AltAlleles[0] == ".";

            // for this to work we need a single-base reference allele and all raw allele counts must be available
            if (sampleFields.TotalAlleleCount == null || isReference || !isRefSingleBase || !areAllAltsSingleBase) return null;

            int numAltAlleles = sampleFields.AltAlleles.Length;
            double[] variantFreqs = new double[numAltAlleles];

            if (sampleFields.TotalAlleleCount == 0) return variantFreqs;

            for (int i = 0; i < numAltAlleles; i++)
            {
                var alleleCount = GetAlleleCount(sampleFields, i);
                variantFreqs[i] = alleleCount / (double)sampleFields.TotalAlleleCount;
            }

            return variantFreqs;
        }

        private static int GetAlleleCount(IntermediateSampleFields sampleFields, int alleleIndex)
        {
            string altAllele = sampleFields.AltAlleles[alleleIndex];
            int alleleCount = 0;

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

        private static double[] GetVariantFrequenciesUsingNrNv(IntermediateSampleFields sampleFields)
        {
            // NR and NV: never observed with multiple alternate alleles
            if (sampleFields.NR == null || sampleFields.NV == null || sampleFields.AltAlleles.Length > 1) return null;
            if (sampleFields.NR == 0) return ZeroVf;

            var nr = (double)sampleFields.NR;
            var nv = (double)sampleFields.NV;
            return new[] { nv / nr };
        }

        private static double[] GetVariantFrequenciesUsingAlleleDepths(IntermediateSampleFields sampleFields)
        {
            if (sampleFields.FormatIndices.AD == null || sampleFields.SampleColumns.Length <= sampleFields.FormatIndices.AD.Value) return null;

            int numAltAlleles     = sampleFields.AltAlleles.Length;
            double[] variantFreqs = new double[numAltAlleles];

            var adField = sampleFields.SampleColumns[sampleFields.FormatIndices.AD.Value];
            var (alleleDepths, allValuesAreValid, totalDepth) = GetAlleleDepths(adField);
            if (!allValuesAreValid || numAltAlleles != alleleDepths.Length) return null;

            // sanity check: make sure we handle NaNs properly
            if (totalDepth == 0) return variantFreqs;

            for (int alleleIndex = 0; alleleIndex < numAltAlleles; alleleIndex++)
            {
                variantFreqs[alleleIndex] = alleleDepths[alleleIndex] / (double)totalDepth;
            }

            return variantFreqs;
        }

        private static (int[] AlleleDepths, bool AllValuesAreValid, int totalDepth) GetAlleleDepths(string adField)
        {
            var adFields = adField.Split(",");
            var alleleDepths = new int[adFields.Length - 1];
            int totalDepth = 0;

            for (int i = 0; i < adFields.Length; i++)
            {
                if (!int.TryParse(adFields[i], out var ad)) return (null, false, totalDepth);
                if (i > 0) alleleDepths[i - 1] = ad;
                totalDepth += ad;
            }

            return (alleleDepths, true, totalDepth);
        }
    }
}
