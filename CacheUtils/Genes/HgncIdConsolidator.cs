using System;
using System.Collections.Generic;
using System.Linq;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CacheUtils.Genes.Utilities;

namespace CacheUtils.Genes
{
    public static class HgncIdConsolidator
    {
        public static int Consolidate(this Dictionary<ushort, List<MutableGene>> genesByRef)
        {
            var numHgncIds = 0;

            foreach (var refKvp in genesByRef.OrderBy(x => x.Key))
            {
                var genesByHgncId = refKvp.Value.Where(gene => gene.HgncId != -1).GetMultiValueDict(x => x.HgncId);

                foreach (var kvp in genesByHgncId)
                {
                    if (kvp.Value.Count <= 1) continue;
                    CreateAggregateGene(kvp.Value.OrderBy(x => x.Start).ThenBy(x => x.End).ToList());
                }

                numHgncIds += refKvp.Value.Count(gene => gene.HgncId != -1);
            }

            return numHgncIds;
        }

        private static void CreateAggregateGene(IReadOnlyList<MutableGene> genes)
        {
            var seedGene = genes[0];
            for (var i = 1; i < genes.Count; i++)
            {
                genes[i].GeneId = null;
                genes[i].HgncId = -1;
                seedGene.End = Math.Max(seedGene.End, genes[i].End);
            }
        }
    }
}
