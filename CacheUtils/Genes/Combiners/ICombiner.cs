using System.Collections.Generic;
using CacheUtils.Genes.DataStructures;

namespace CacheUtils.Genes.Combiners
{
    public interface ICombiner
    {
        void Combine(List<UgaGene> combinedGenes, HashSet<UgaGene> remainingGenes37, HashSet<UgaGene> remainingGenes38);
    }
}
