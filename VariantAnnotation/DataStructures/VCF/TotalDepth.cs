namespace VariantAnnotation.DataStructures.VCF
{
    internal sealed class TotalDepth
    {
        #region members

        private readonly IIntermediateSampleFields _tmp;

        #endregion

        // constructor
        public TotalDepth(IIntermediateSampleFields intermediateSampleFields)
        {
            _tmp = intermediateSampleFields;
        }

        /// <summary>
        /// returns the total depth given different sources of information
        /// </summary>
        public string GetTotalDepth(int? infoDepth)
        {
            // use TAR & TIR
            if (_tmp.Tar != null && _tmp.Tir != null) return GetTotalDepthUsingTarTir();

            // use base counts
            if (_tmp.TotalAlleleCount != null) return GetTotalDepthUsingAlleleCounts();

            // use DPI
            if (_tmp.FormatIndices.DPI != null) return GetTotalDepthUsingDpi();

            // use DP
            if (_tmp.FormatIndices.DP != null) return GetTotalDepthUsingDp();

            // use NR
            if (_tmp.NR != null && _tmp.AltAlleles.Length == 1) return _tmp.NR.ToString();

            // use INFO DP (Pisces)
            return infoDepth?.ToString();
        }

        /// <summary>
        /// returns the total depth using TAR & TIR
        /// </summary>
        private string GetTotalDepthUsingTarTir()
        {
            return (_tmp.Tar + _tmp.Tir).ToString();
        }

        /// <summary>
        /// returns the total depth using tier 1 allele counts
        /// </summary>
        private string GetTotalDepthUsingAlleleCounts()
        {
            return _tmp.TotalAlleleCount.ToString();
        }

        /// <summary>
        /// returns the total depth using DPI
        /// </summary>
        private string GetTotalDepthUsingDpi()
        {
            if (_tmp.FormatIndices.DPI == null || _tmp.SampleColumns.Length <= _tmp.FormatIndices.DPI.Value) return null;
            var depth = _tmp.SampleColumns[_tmp.FormatIndices.DPI.Value];
            return depth == "." ? null : depth;
        }

        /// <summary>
        /// returns the total depth using DP
        /// </summary>
        private string GetTotalDepthUsingDp()
        {
            if (_tmp.FormatIndices.DP == null || _tmp.SampleColumns.Length <= _tmp.FormatIndices.DP.Value) return null;
            var depth = _tmp.SampleColumns[_tmp.FormatIndices.DP.Value];
            return depth == "." ? null : depth;
        }
    }
}
