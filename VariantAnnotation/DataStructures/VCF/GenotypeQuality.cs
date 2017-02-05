namespace VariantAnnotation.DataStructures.VCF
{
    internal sealed class GenotypeQuality
    {
        #region members

        private readonly IIntermediateSampleFields _tmp;

        #endregion

        // constructor
        public GenotypeQuality(IIntermediateSampleFields intermediateSampleFields)
        {
            _tmp = intermediateSampleFields;
        }

        /// <summary>
        /// returns the genotype quality given different sources of information
        /// </summary>
        public string GetGenotypeQuality()
        {
            var hasGqx = _tmp.FormatIndices.GQX != null;
            var hasGq  = _tmp.FormatIndices.GQ != null;

            if (!hasGqx && !hasGq)  return null;

            var gqIndex = hasGqx ? _tmp.FormatIndices.GQX.Value : _tmp.FormatIndices.GQ.Value;
            if (_tmp.SampleColumns.Length <= gqIndex) return null;

            var gq = _tmp.SampleColumns[gqIndex];
            return gq == "." || gq == "./." ? null : gq;
        }
    }
}
