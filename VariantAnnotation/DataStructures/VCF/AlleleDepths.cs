using System.Linq;

namespace VariantAnnotation.DataStructures.VCF
{
    internal sealed class AlleleDepths
    {
        #region members

        private readonly IIntermediateSampleFields _tmp;

        #endregion

        // constructor
        public AlleleDepths(IIntermediateSampleFields intermediateSampleFields)
        {
            _tmp = intermediateSampleFields;
        }

        /// <summary>
        /// returns the allele depths given different sources of information
        /// </summary>
        public string[] GetAlleleDepths()
        {
            string[] ad = null;

            // use TAR & TIR
            if (_tmp.Tar != null && _tmp.Tir != null) ad = GetAlleleDepthsUsingTarTir();

            // use allele counts
            if (ad == null && _tmp.TotalAlleleCount != null) ad = GetAlleleDepthsUsingAlleleCounts();

            // use allele depths
            if (ad == null && _tmp.FormatIndices.AD != null) ad = GetAlleleDepthsUsingAd();

            // use NR & NV
            if (ad == null && _tmp.NR != null && _tmp.NV != null) ad = GetAlleleDepthsUsingNrNv();

            return ad;
        }

        /// <summary>
        /// returns the variant frequency using TIR and TAR
        /// </summary>
        private string[] GetAlleleDepthsUsingTarTir()
        {
            if (_tmp.Tir == null || _tmp.Tar == null || _tmp.AltAlleles.Length > 1) return null;
            return new[] { _tmp.Tar.ToString(), _tmp.Tir.ToString() };
        }

        /// <summary>
        /// returns the allele depths using allele counts
        /// </summary>
        private string[] GetAlleleDepthsUsingAlleleCounts()
        {
            if (_tmp.TotalAlleleCount == null) return null;

            // sanity check: make sure all alternate alleles are SNVs
            if (_tmp.VcfRefAllele.Length != 1 || _tmp.AltAlleles.Any(altAllele => altAllele.Length != 1)) return null;

            var ad = new string[_tmp.AltAlleles.Length + 1];

            // handle reference allele
            var ac = GetAlleleCountString(_tmp.VcfRefAllele);
            if (ac == null) return null;
            ad[0] = ac;

            // handle alternate alleles
            var index = 1;
            foreach (var altAllele in _tmp.AltAlleles)
            {
                ac = GetAlleleCountString(altAllele);
                if (ac == null) return null;
                ad[index++] = ac;
            }

            return ad;
        }

        /// <summary>
        /// returns the appropriate allele count string given the supplied base
        /// </summary>
        private string GetAlleleCountString(string s)
        {
            string ac = null;

            switch (s)
            {
                case "A":
                    ac = _tmp.ACount.ToString();
                    break;
                case "C":
                    ac = _tmp.CCount.ToString();
                    break;
                case "G":
                    ac = _tmp.GCount.ToString();
                    break;
                case "T":
                    ac = _tmp.TCount.ToString();
                    break;
            }

            return ac;
        }

        /// <summary>
        /// returns the allele depths using allele depths
        /// </summary>
        private string[] GetAlleleDepthsUsingAd()
        {
            if (_tmp.FormatIndices.AD == null || _tmp.SampleColumns.Length <= _tmp.FormatIndices.AD.Value) return null;
            var ad = _tmp.SampleColumns[_tmp.FormatIndices.AD.Value].Split(',');
            return ad[0] == "." ? null : ad;
        }

        /// <summary>
        /// returns the allele depths using NR & NV from Platypus
        /// </summary>
        private string[] GetAlleleDepthsUsingNrNv()
        {
            return _tmp.AltAlleles.Length > 1 ? null : new[] { (_tmp.NR - _tmp.NV).ToString(), _tmp.NV.ToString() };
        }
    }
}
