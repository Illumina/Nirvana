﻿using OptimizedCore;

namespace Vcf.Sample.Legacy
{
    internal static class AlleleDepths
    {
        /// <summary>
        /// returns the allele depths given different sources of information
        /// </summary>
        public static int[] GetAlleleDepths(IntermediateSampleFields intermediateSampleFields)
        {
            int[] ad = null;

            // use TAR & TIR
            if (intermediateSampleFields.TAR != null && intermediateSampleFields.TIR != null) ad = GetAlleleDepthsUsingTarTir(intermediateSampleFields);

            // use allele counts
            if (ad == null && intermediateSampleFields.TotalAlleleCount != null) ad = GetAlleleDepthsUsingAlleleCounts(intermediateSampleFields);

            // use allele depths
            if (ad == null && intermediateSampleFields.FormatIndices.AD != null) ad = GetAlleleDepthsUsingAd(intermediateSampleFields);

            return ad;
        }

        /// <summary>
        /// returns the variant frequency using TIR and TAR
        /// </summary>
        private static int[] GetAlleleDepthsUsingTarTir(IntermediateSampleFields intermediateSampleFields)
        {
            if (intermediateSampleFields.TIR == null || intermediateSampleFields.TAR == null || intermediateSampleFields.AltAlleles.Length > 1) return null;
            return new[] { intermediateSampleFields.TAR.Value, intermediateSampleFields.TIR.Value };
        }

        /// <summary>
        /// returns the allele depths using allele counts
        /// </summary>
        private static int[] GetAlleleDepthsUsingAlleleCounts(IntermediateSampleFields intermediateSampleFields)
        {
            if (intermediateSampleFields.TotalAlleleCount == null) return null;

            // sanity check: make sure all alternate alleles are SNVs
            if (intermediateSampleFields.VcfRefAllele.Length != 1 || !intermediateSampleFields.AltAlleles.AreAllAltAllelesSingleBase()) return null;

            var ad = new int[intermediateSampleFields.AltAlleles.Length + 1];

            // handle reference allele
            var ac = GetAlleleCountString(intermediateSampleFields.VcfRefAllele, intermediateSampleFields);
            if (ac == null) return null;
            ad[0] = ac.Value;

            // handle alternate alleles
            var index = 1;

            foreach (string altAllele in intermediateSampleFields.AltAlleles)
            {
                ac = GetAlleleCountString(altAllele, intermediateSampleFields);
                if (ac == null) return null;
                ad[index++] = ac.Value;
            }

            return ad;
        }

        /// <summary>
        /// returns the appropriate allele count string given the supplied base
        /// </summary>
        private static int? GetAlleleCountString(string s, IntermediateSampleFields intermediateSampleFields)
        {
            int? ac = null;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (s)
            {
                case "A":
                    ac = intermediateSampleFields.ACount;
                    break;
                case "C":
                    ac = intermediateSampleFields.CCount;
                    break;
                case "G":
                    ac = intermediateSampleFields.GCount;
                    break;
                case "T":
                    ac = intermediateSampleFields.TCount;
                    break;
            }

            return ac;
        }

        /// <summary>
        /// returns the allele depths using allele depths
        /// </summary>
        private static int[] GetAlleleDepthsUsingAd(IntermediateSampleFields intermediateSampleFields)
        {
            if (intermediateSampleFields.FormatIndices.AD == null || intermediateSampleFields.SampleColumns.Length <=
                intermediateSampleFields.FormatIndices.AD.Value) return null;

            var ad = intermediateSampleFields.SampleColumns[intermediateSampleFields.FormatIndices.AD.Value].OptimizedSplit(',');
            if (ad[0] == ".") return null;

            int nAllele = ad.Length;
            var alleleDepths = new int[nAllele];

            for (var i = 0; i < nAllele; i++)
            {
                (int number, bool foundError) = ad[i].OptimizedParseInt32();
                if (foundError) return null;
                alleleDepths[i] = number;
            }

            return alleleDepths;
        }
    }
}
