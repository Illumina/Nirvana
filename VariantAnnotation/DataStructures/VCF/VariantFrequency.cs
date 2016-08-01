namespace VariantAnnotation.DataStructures.VCF
{
    internal class VariantFrequency
    {
        #region members

        private readonly IIntermediateSampleFields _tmp;

        #endregion

        // constructor
        public VariantFrequency(IIntermediateSampleFields intermediateSampleFields)
        {
            _tmp = intermediateSampleFields;
        }

        /// <summary>
        /// returns the variant frequency given different sources of information
        /// </summary>
        public string GetVariantFrequency()
        {
            string vf = null;

            // use TAR & TIR
            if ((_tmp.Tar != null) && (_tmp.Tir != null)) vf = GetVariantFrequencyUsingTarTir();

            // use allele counts
            if ((vf == null) && (_tmp.TotalAlleleCount != null)) vf = GetVariantFrequencyUsingAlleleCounts();

            // use allele depths
            if ((vf == null) && (_tmp.FormatIndices.AD != null)) vf = GetVariantFrequencyUsingAlleleDepths();

            // use NR & NV
            if ((vf == null) && (_tmp.NR != null) && (_tmp.NV != null)) vf = GetVariantFrequencyUsingNrNv();

            return vf;
        }

        /// <summary>
        /// returns the variant frequency using raw allele counts
        /// </summary>
        private string GetVariantFrequencyUsingAlleleCounts()
        {
            // for this to work we need a single-base reference allele and all raw allele
            // counts must be available
            if ((_tmp.TotalAlleleCount == null) || (_tmp.VcfRefAllele == null) || (_tmp.VcfRefAllele.Length != 1)) return null;

	        if (_tmp.TotalAlleleCount == 0) return "0";
			
            // get the reference count
            int? refCount = null;

            switch (_tmp.VcfRefAllele)
            {
                case "A":
                    refCount = _tmp.ACount;
                    break;
                case "C":
                    refCount = _tmp.CCount;
                    break;
                case "G":
                    refCount = _tmp.GCount;
                    break;
                case "T":
                    refCount = _tmp.TCount;
                    break;
            }

            // sanity check: make sure we have a canonical base (A,C,G,T)
            if (refCount == null) return null;

            // calculate the variant frequency
            double vf = ((double)_tmp.TotalAlleleCount - (double)refCount) / (double)_tmp.TotalAlleleCount;
            return vf.ToString("0.####");
        }

        /// <summary>
        /// returns the variant frequency using TIR and TAR
        /// </summary>
        private string GetVariantFrequencyUsingTarTir()
        {
            if ((_tmp.Tir == null) || (_tmp.Tar == null)) return null;
	        if (_tmp.Tir + _tmp.Tar == 0) return "0";
            double tir = (double)_tmp.Tir;
            double tar = (double)_tmp.Tar;
            double vf = tir / (tar + tir);
            return vf.ToString("0.####");
        }

        /// <summary>
        /// returns the variant frequency using TIR and TAR
        /// </summary>
        private string GetVariantFrequencyUsingNrNv()
        {
            if ((_tmp.NR == null) || (_tmp.NV == null) || (_tmp.AltAlleles.Length > 1)) return null;
            if (_tmp.NR == 0) return "0";
            var nr = (double)_tmp.NR;
            var nv = (double)_tmp.NV;
            return (nv / nr).ToString("0.####");
        }

        /// <summary>
        /// returns the variant frequency using allele depths
        /// </summary>
        private string GetVariantFrequencyUsingAlleleDepths()
        {
            if ((_tmp.FormatIndices.AD == null) || (_tmp.SampleColumns.Length <= _tmp.FormatIndices.AD.Value)) return null;
            var alleleDepths = _tmp.SampleColumns[_tmp.FormatIndices.AD.Value].Split(',');

            // one allele depth
            if (alleleDepths.Length == 1)
            {
                int num;
                return int.TryParse(alleleDepths[0], out num) ? "0" : null;
            }

            bool hasRefDepth = false;
            int refDepth = 0;
            int totalDepth = 0;

            foreach (var depthString in alleleDepths)
            {
                int depth;
                if (!int.TryParse(depthString, out depth)) return null;

                totalDepth += depth;

                if (!hasRefDepth)
                {
                    refDepth = depth;
                    hasRefDepth = true;
                }
            }

            // sanity check: make sure we handle NaNs properly
            if (totalDepth == 0) return "0";

            double vf = (totalDepth - refDepth) / (double)totalDepth;
            return vf.ToString("0.####");
        }
    }
}
