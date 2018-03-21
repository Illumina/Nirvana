using System.Collections.Generic;
using CacheUtils.TranscriptCache.Comparers;
using ErrorHandling.Exceptions;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.Commands.GFF
{
    public static class InternalGenes
    {
        public static IDictionary<IGene, int> CreateDictionary(IGene[] genes)
        {
            var geneComparer     = new GeneComparer();
            var geneToInternalId = new Dictionary<IGene, int>(geneComparer);

            for (var geneIndex = 0; geneIndex < genes.Length; geneIndex++)
            {
                var gene = genes[geneIndex];

                if (geneToInternalId.TryGetValue(gene, out int oldGeneIndex))
                {
                    throw new UserErrorException($"Found a duplicate gene in the dictionary: {genes[geneIndex]} ({geneIndex} vs {oldGeneIndex})");
                }

                geneToInternalId[gene] = geneIndex;
            }

            return geneToInternalId;
        }
    }
}
