using System.Collections.Generic;

namespace VariantAnnotation.DataStructures.VCF
{
    internal sealed class RecomposedGenotype
    {
        #region members

        private readonly IIntermediateSampleFields _tmp;
        private readonly List<string> _recomposedVariantIds;

        #endregion

        public RecomposedGenotype(IIntermediateSampleFields intermediateSampleFields, List<string> recomposedVariantIds)
        {
            _tmp = intermediateSampleFields;
            _recomposedVariantIds = recomposedVariantIds;
        }

        public List<string> GetRecomposedGenotype()
        {
            if (_tmp.FormatIndices.RGT == null) return null;

            var recomposedGenotypeString = _tmp.SampleColumns[_tmp.FormatIndices.RGT.Value];
            var genotypeIndices          = recomposedGenotypeString.Split('|', '/');

            var genotypes = new List<string>();

            foreach (var genotypeIndex in genotypeIndices)
            {
                if (!int.TryParse(genotypeIndex, out int index)) continue;
                if (index < 1) continue;
                genotypes.Add(_recomposedVariantIds[index - 1]);
            }

            return genotypes;
        }
    }
}