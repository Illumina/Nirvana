using System.Collections.Generic;
using CacheUtils.Genes.DataStructures;
using CacheUtils.Genes.Utilities;

namespace CacheUtils.Genes.Combiners
{
    public sealed class PartitionCombiner : ICombiner
    {
        public void Combine(List<UgaGene> combinedGenes, HashSet<UgaGene> remainingGenes37,
            HashSet<UgaGene> remainingGenes38)
        {
            var grch37 = Partition(remainingGenes37);
            var grch38 = Partition(remainingGenes38);

            CombineSet(combinedGenes, grch37.Both, grch38.Both, remainingGenes37, remainingGenes38);
            CombineSet(combinedGenes, grch37.EntrezGeneOnly, grch38.EntrezGeneOnly, remainingGenes37, remainingGenes38);
            CombineSet(combinedGenes, grch37.EnsemblOnly, grch38.EnsemblOnly, remainingGenes37, remainingGenes38);
        }

        private static void CombineSet(ICollection<UgaGene> combinedGenes, IEnumerable<UgaGene> uga37,
            IEnumerable<UgaGene> uga38, ICollection<UgaGene> remainingGenes37, ICollection<UgaGene> remainingGenes38)
        {
            var keyToGene37 = uga37.GetMultiValueDict(GetKey);
            var keyToGene38 = uga38.GetMultiValueDict(GetKey);
            var keys        = GetAllKeys(keyToGene37.Keys, keyToGene38.Keys);

            foreach (var key in keys)
            {
                var genes37 = GetGenesByKey(keyToGene37, key);
                var genes38 = GetGenesByKey(keyToGene38, key);

                CombinerUtils.RemoveGenes(genes37, remainingGenes37);
                CombinerUtils.RemoveGenes(genes38, remainingGenes38);

                // this happens for both Entrez Gene Only & Ensembl Only
                if (genes37.Count == 1 && genes38.Count == 1)
                {
                    var gene37 = genes37[0];
                    var gene38 = genes38[0];

                    var mergedGene = CombinerUtils.Merge(gene37, gene38);
                    combinedGenes.Add(mergedGene);
                    continue;
                }

                // the following situations happen if we have:
                // - one gene from GRCh37 and none from GRCh38 (or vice versa)
                // - two or more non-overlapping genes on the same assembly (14 occurrences)
                CombinerUtils.AddOrphans(combinedGenes, genes37);
                CombinerUtils.AddOrphans(combinedGenes, genes38);
            }
        }

        private static List<UgaGene> GetGenesByKey(IReadOnlyDictionary<string, List<UgaGene>> genesByKey, string key) =>
            genesByKey.TryGetValue(key, out var genes) ? genes : UgaAssemblyCombiner.EmptyUgaGenes;

        private static IEnumerable<string> GetAllKeys(IEnumerable<string> keys37, IEnumerable<string> keys38)
        {
            var keys = new HashSet<string>();
            foreach (var key in keys37) keys.Add(key);
            foreach (var key in keys38) keys.Add(key);
            return keys;
        }

        private static string GetKey(UgaGene gene) =>
            gene.EnsemblId + '|' + gene.EntrezGeneId + '|' + (gene.OnReverseStrand ? "R" : "F");

        private static (List<UgaGene> EnsemblOnly, List<UgaGene> Both, List<UgaGene> EntrezGeneOnly) Partition(
            IEnumerable<UgaGene> remainingGenes)
        {
            var ensemblOnly    = new List<UgaGene>();
            var both           = new List<UgaGene>();
            var entrezGeneOnly = new List<UgaGene>();

            foreach (var gene in remainingGenes)
            {
                if (gene.EntrezGeneId != null && gene.EnsemblId != null) both.Add(gene);
                else if (gene.EntrezGeneId != null) entrezGeneOnly.Add(gene);
                else ensemblOnly.Add(gene);
            }

            return (ensemblOnly, both, entrezGeneOnly);
        }
    }
}
