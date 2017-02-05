namespace VariantAnnotation.DataStructures.VCF
{
    internal sealed class Genotype
    {
        #region members

        private readonly IIntermediateSampleFields _tmp;

        #endregion

        // constructor
        public Genotype(IIntermediateSampleFields intermediateSampleFields)
        {
            _tmp = intermediateSampleFields;
        }

        /// <summary>
        /// returns the genotype flag
        /// </summary>
        public string GetGenotype()
        {
            if (_tmp.FormatIndices.GT == null) return null;
            var genotype = _tmp.SampleColumns[_tmp.FormatIndices.GT.Value];
            return genotype == "." ? null : genotype;
        }
    }
}
