using System.Collections.Generic;
using System.IO;
using CacheUtils.Genes.DataStructures;

namespace CacheUtils.Genes.Combiners
{
    public static class CombinerUtils
    {
        public static UgaGene Merge(UgaGene gene37, UgaGene gene38)
        {
            var ensemblId    = CombineField(gene37.EnsemblId,    gene38.EnsemblId);
            var entrezGeneId = CombineField(gene37.EntrezGeneId, gene38.EntrezGeneId);
            var hgncId       = CombineField(gene37.HgncId,       gene38.HgncId);
            return new UgaGene(gene37.Chromosome, gene37.GRCh37, gene38.GRCh38, gene37.OnReverseStrand, entrezGeneId,
                ensemblId, gene37.Symbol, hgncId);
        }

        private static T CombineField<T>(T grch37, T grch38)
        {
            if (grch37 == null) return grch38;
            if (grch38 == null) return grch37;
            if (!grch37.Equals(grch38)) throw new InvalidDataException($"Found two different values: {grch37} & {grch38}");
            return grch37;
        }

        internal static void RemoveGenes(IEnumerable<UgaGene> genes, ICollection<UgaGene> remainingGenes)
        {
            foreach (var gene in genes) remainingGenes.Remove(gene);
        }

        internal static void AddOrphans(ICollection<UgaGene> combinedGenes, IEnumerable<UgaGene> genes)
        {
            foreach (var gene in genes) combinedGenes.Add(gene);
        }
    }
}
