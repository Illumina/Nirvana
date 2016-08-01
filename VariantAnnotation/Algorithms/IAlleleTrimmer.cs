using System.Collections.Generic;
using VariantAnnotation.DataStructures;

namespace VariantAnnotation.Algorithms
{
    public interface IAlleleTrimmer
    {
        /// <summary>
        /// trims both the reference and the alternate allele
        /// </summary>
        void Trim(List<VariantAlternateAllele> alternateAlleles);
    }
}
