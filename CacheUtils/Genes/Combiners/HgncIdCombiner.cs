using System.Collections.Generic;
using CacheUtils.Genes.DataStructures;
using CacheUtils.Genes.Utilities;

namespace CacheUtils.Genes.Combiners
{
    public sealed class HgncIdCombiner : ICombiner
    {
        public void Combine(List<UgaGene> combinedGenes, HashSet<UgaGene> remainingGenes37,
            HashSet<UgaGene> remainingGenes38)
        {
            var hgncIds       = GetHgncIds(remainingGenes37, remainingGenes38);
            var genesByHgnc37 = remainingGenes37.GetMultiValueDict(x => x.HgncId);
            var genesByHgnc38 = remainingGenes38.GetMultiValueDict(x => x.HgncId);

            foreach (int hgncId in hgncIds)
            {
                var genes37 = GetGenesByHgncId(genesByHgnc37, hgncId);
                var genes38 = GetGenesByHgncId(genesByHgnc38, hgncId);

                CombinerUtils.RemoveGenes(genes37, remainingGenes37);
                CombinerUtils.RemoveGenes(genes38, remainingGenes38);

                // merge if we have one gene on each genome assembly and they're on the same strand
                if (genes37.Count == 1 && genes38.Count == 1)
                {
                    var gene37 = genes37[0];
                    var gene38 = genes38[0];

                    if (gene37.OnReverseStrand == gene38.OnReverseStrand)
                    {
                        var mergedGene = CombinerUtils.Merge(gene37, gene38);
                        combinedGenes.Add(mergedGene);
                        continue;
                    }
                }

                // the following situations happen if we have:
                // - one gene from GRCh37 and none from GRCh38 (or vice versa)
                // - there is a mixture of genes forward and reverse strands (13 occurrences)
                CombinerUtils.AddOrphans(combinedGenes, genes37);
                CombinerUtils.AddOrphans(combinedGenes, genes38);
            }
        }

        private static List<UgaGene> GetGenesByHgncId(IReadOnlyDictionary<int, List<UgaGene>> genesByHgnc, int hgncId) =>
            genesByHgnc.TryGetValue(hgncId, out var genes) ? genes : UgaAssemblyCombiner.EmptyUgaGenes;

        private static IEnumerable<int> GetHgncIds(IEnumerable<UgaGene> remainingUga37, IEnumerable<UgaGene> remainingUga38)
        {
            var hgncIds = new HashSet<int>();
            foreach (var gene in remainingUga37) if (gene.HgncId != -1) hgncIds.Add(gene.HgncId);
            foreach (var gene in remainingUga38) if (gene.HgncId != -1) hgncIds.Add(gene.HgncId);
            return hgncIds;
        }
    }
}
