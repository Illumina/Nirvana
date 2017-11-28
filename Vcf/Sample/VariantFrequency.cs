namespace Vcf.Sample
{
    internal static class VariantFrequency
    {
        /// <summary>
        /// returns the variant frequency given different sources of information
        /// </summary>
        public static double? GetVariantFrequency(IntermediateSampleFields intermediateSampleFields)
        {
            double? vf = null;

            // use TAR & TIR
            if (intermediateSampleFields.Tar != null && intermediateSampleFields.Tir != null) vf = GetVariantFrequencyUsingTarTir(intermediateSampleFields);

            // use allele counts
            if (vf == null && intermediateSampleFields.TotalAlleleCount != null) vf = GetVariantFrequencyUsingAlleleCounts(intermediateSampleFields);

            // use allele depths
            if (vf == null && intermediateSampleFields.FormatIndices.AD != null) vf = GetVariantFrequencyUsingAlleleDepths(intermediateSampleFields);

            // use NR & NV
            if (vf == null && intermediateSampleFields.NR != null && intermediateSampleFields.NV != null) vf = GetVariantFrequencyUsingNrNv(intermediateSampleFields);

            return vf;
        }

        /// <summary>
        /// returns the variant frequency using raw allele counts
        /// </summary>
        private static  double? GetVariantFrequencyUsingAlleleCounts(IntermediateSampleFields intermediateSampleFields)
        {
            // for this to work we need a single-base reference allele and all raw allele
            // counts must be available
            if (intermediateSampleFields.TotalAlleleCount == null || intermediateSampleFields.VcfRefAllele == null || intermediateSampleFields.VcfRefAllele.Length != 1) return null;

	        if (intermediateSampleFields.TotalAlleleCount == 0) return 0;
			
            // get the reference count
            int? refCount = null;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (intermediateSampleFields.VcfRefAllele)
            {
                case "A":
                    refCount = intermediateSampleFields.ACount;
                    break;
                case "C":
                    refCount = intermediateSampleFields.CCount;
                    break;
                case "G":
                    refCount = intermediateSampleFields.GCount;
                    break;
                case "T":
                    refCount = intermediateSampleFields.TCount;
                    break;
            }

            // sanity check: make sure we have a canonical base (A,C,G,T)
            if (refCount == null) return null;

            // calculate the variant frequency
            return  ((double)intermediateSampleFields.TotalAlleleCount - (double)refCount) / (double)intermediateSampleFields.TotalAlleleCount;
        }

        /// <summary>
        /// returns the variant frequency using TIR and TAR
        /// </summary>
        private static  double? GetVariantFrequencyUsingTarTir(IntermediateSampleFields intermediateSampleFields)
        {
            if (intermediateSampleFields.Tir == null || intermediateSampleFields.Tar == null) return null;
	        if (intermediateSampleFields.Tir + intermediateSampleFields.Tar == 0) return 0;
            var tir = (double)intermediateSampleFields.Tir;
            var tar = (double)intermediateSampleFields.Tar;
            return tir / (tar + tir);
        }

        /// <summary>
        /// returns the variant frequency using TIR and TAR
        /// </summary>
        private static  double? GetVariantFrequencyUsingNrNv(IntermediateSampleFields intermediateSampleFields)
        {
            if (intermediateSampleFields.NR == null || intermediateSampleFields.NV == null || intermediateSampleFields.AltAlleles.Length > 1) return null;
            if (intermediateSampleFields.NR == 0) return 0;
            var nr = (double)intermediateSampleFields.NR;
            var nv = (double)intermediateSampleFields.NV;
            return nv / nr;
        }

        /// <summary>
        /// returns the variant frequency using allele depths
        /// </summary>
        private static  double? GetVariantFrequencyUsingAlleleDepths(IntermediateSampleFields intermediateSampleFields)
        {
            if (intermediateSampleFields.FormatIndices.AD == null || intermediateSampleFields.SampleColumns.Length <= intermediateSampleFields.FormatIndices.AD.Value) return null;
            var alleleDepths = intermediateSampleFields.SampleColumns[intermediateSampleFields.FormatIndices.AD.Value].Split(',');

            // one allele depth
            if (alleleDepths.Length == 1)
            {
                if(int.TryParse(alleleDepths[0], out _) ) return 0;
                return null;
            }

            var hasRefDepth = false;
            var refDepth = 0;
            var totalDepth = 0;

            foreach (var depthString in alleleDepths)
            {
                if (!int.TryParse(depthString, out var depth)) return null;

                totalDepth += depth;
                if (hasRefDepth) continue;

                refDepth    = depth;
                hasRefDepth = true;
            }

            // sanity check: make sure we handle NaNs properly
            if (totalDepth == 0) return 0;

            return (totalDepth - refDepth) / (double)totalDepth;
        }
    }
}
