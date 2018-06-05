using System;
using System.Collections.Generic;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CacheUtils.Genes.DataStructures;
using CacheUtils.Genes.Utilities;

namespace CacheUtils.Genes
{
    public static class HgncIdUpdater
    {
        public static Dictionary<ushort, List<MutableGene>> Update(this IEnumerable<HgncGene> hgncGenes,
            Dictionary<ushort, List<MutableGene>> genesByRef, Func<HgncGene, string> idFunc)
        {
            var geneIdToHgncId = hgncGenes.GetSingleValueDict(idFunc);
            foreach (var kvp in genesByRef) ReplaceHgncIds(kvp.Value, geneIdToHgncId);
            return genesByRef;
        }

        private static void ReplaceHgncIds(IEnumerable<MutableGene> genes, IReadOnlyDictionary<string, HgncGene> geneIdToHgncGene)
        {
            foreach (var gene in genes)
            {
                gene.HgncId = -1;
                if (!geneIdToHgncGene.TryGetValue(gene.GeneId, out var hgncGene)) continue;
                if (!Intervals.Utilities.Overlaps(hgncGene.Start, hgncGene.End, gene.Start, gene.End)) continue;

                gene.HgncId = hgncGene.HgncId;
            }
        }
    }
}
