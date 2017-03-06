namespace VariantAnnotation.DataStructures.VCF
{
    internal sealed class FailedFilter
    {
        #region members

        private readonly IIntermediateSampleFields _tmp;

        #endregion

        // constructor
        public FailedFilter(IIntermediateSampleFields intermediateSampleFields)
        {
            _tmp = intermediateSampleFields;
        }

        /// <summary>
        /// returns the failed filter flag
        /// </summary>
        public bool GetFailedFilter()
        {
            if (_tmp.FormatIndices.FT == null) return false;
            var filterValue = _tmp.SampleColumns[_tmp.FormatIndices.FT.Value];
            return filterValue != "PASS" && filterValue != ".";
        }
    }
}
