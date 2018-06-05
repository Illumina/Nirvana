using System;
using System.Collections.Generic;
using System.Linq;
using CacheUtils.Genes.DataStructures;

namespace CacheUtils.Genes
{
    public static class GeneFlattener
    {
        public static List<T> FlattenGeneList<T>(this Dictionary<string, List<T>> genesById) where T : IFlatGene<T>
        {
            var genesList = new List<T>();

            foreach (var genes in genesById.Values)
            {
                var flatGenes = FlattenWithSameId(genes);
                genesList.AddRange(flatGenes);
            }

            return genesList.OrderBy(x => x.Chromosome.Index).ThenBy(x => x.Start).ThenBy(x => x.End).ToList();
        }

        internal static List<T> FlattenWithSameId<T>(List<T> genes) where T : IFlatGene<T>
        {
            if (genes == null || genes.Count == 1) return genes;

            var flatGenes = new List<T>();
            var seedGene  = genes[0].Clone();

            foreach (var gene in genes)
            {
                if (Intervals.Utilities.Overlaps(seedGene.Start, seedGene.End, gene.Start, gene.End))
                {
                    seedGene.End = Math.Max(seedGene.End, gene.End);
                    continue;
                }

                flatGenes.Add(seedGene);
                seedGene = gene.Clone();
            }

            flatGenes.Add(seedGene);
            return flatGenes;
        }
    }
}
