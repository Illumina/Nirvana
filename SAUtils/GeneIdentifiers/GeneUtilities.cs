using System;
using System.Collections.Generic;
using CacheUtils.Genes.DataStructures;
using CacheUtils.Genes.IO;
using Compression.Utilities;
using Genome;
using ReferenceSequence.Utilities;

namespace SAUtils.GeneIdentifiers
{
    public static class GeneUtilities
    {
        public static string GetGeneSymbolFromId(string geneId, Dictionary<string, string> entrezGeneIdToSymbol, Dictionary<string, string> ensemblIdToSymbol)
        {
            string geneSymbol;

            if (geneId.StartsWith("ENSG")) return ensemblIdToSymbol.TryGetValue(geneId, out geneSymbol) ? geneSymbol : null;
            
            return entrezGeneIdToSymbol.TryGetValue(geneId, out geneSymbol) ? geneSymbol : null; 
        }

        public static (Dictionary<string, string> EntrezGeneIdToSymbol, Dictionary<string, string> EnsemblIdToSymbol) ParseUniversalGeneArchive(string inputReferencePath, string universalGeneArchivePath)
        {

            IDictionary<string, IChromosome> refNameToChromosome;
            if (inputReferencePath == null) refNameToChromosome = null;
            else (_, refNameToChromosome, _) = SequenceHelper.GetDictionaries(inputReferencePath);

            UgaGene[] genes;

            using (var reader = new UgaGeneReader(GZipUtilities.GetAppropriateReadStream(universalGeneArchivePath),
                refNameToChromosome))
            {
                genes = reader.GetGenes();
            }

            var entrezGeneIdToSymbol = genes.GetGeneIdToSymbol(x => x.EntrezGeneId);
            var ensemblIdToSymbol = genes.GetGeneIdToSymbol(x => x.EnsemblId);
            return (entrezGeneIdToSymbol, ensemblIdToSymbol);
        }

        private static Dictionary<string, string> GetGeneIdToSymbol(this IEnumerable<UgaGene> genes,
            Func<UgaGene, string> geneIdFunc)
        {
            var dict = new Dictionary<string, string>();
            foreach (var gene in genes)
            {
                var key = geneIdFunc(gene);
                var symbol = gene.Symbol;
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(symbol)) continue;
                dict[key] = symbol;
            }
            return dict;
        }
    }
}