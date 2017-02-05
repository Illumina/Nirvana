using System.Collections.Generic;
using System.Linq;
using CacheUtils.CombineAndUpdateGenes.DataStructures;
using VariantAnnotation.DataStructures;

namespace CacheUtils.CombineAndUpdateGenes.Utilities
{
    public static class GeneUtilities
    {
        public static Dictionary<string, List<MutableGene>> GetGenesById(List<MutableGene> genes, bool isEnsembl)
        {
            var genesById = new Dictionary<string, List<MutableGene>>();

            foreach (var gene in genes)
            {
                var geneId = isEnsembl
                    ? gene.EnsemblId.ToString()
                    : gene.EntrezGeneId.ToString();

                List<MutableGene> oldGenes;
                if (genesById.TryGetValue(geneId, out oldGenes)) oldGenes.Add(gene);
                else genesById[geneId] = new List<MutableGene> { gene };
            }

            return genesById;
        }

        public static MutableGene GetRefSeqGeneById(List<MutableGene> genes, string entrezGeneId)
        {
            return genes.FirstOrDefault(gene => gene.EntrezGeneId.ToString() == entrezGeneId);
        }

        public static Dictionary<string, List<MutableGene>> GetGenesBySymbol(List<MutableGene> genes)
        {
            var genesBySymbol = new Dictionary<string, List<MutableGene>>();

            foreach (var gene in genes)
            {
                List<MutableGene> oldGenes;
                if (genesBySymbol.TryGetValue(gene.Symbol, out oldGenes)) oldGenes.Add(gene);
                else genesBySymbol[gene.Symbol] = new List<MutableGene> { gene };
            }

            return genesBySymbol;
        }

        public static List<MutableGene> GetGenesByDataSource(List<MutableGene> genes, TranscriptDataSource desiredDataSource)
        {
            return genes.Where(gene => gene.TranscriptDataSource == desiredDataSource).ToList();
        }
    }
}
