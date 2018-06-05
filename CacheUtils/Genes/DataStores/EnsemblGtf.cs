using System.Collections.Generic;
using System.IO;
using CacheUtils.Genes.DataStructures;
using CacheUtils.Genes.IO;
using CacheUtils.Genes.Utilities;
using Compression.Utilities;
using Genome;

namespace CacheUtils.Genes.DataStores
{
    public sealed class EnsemblGtf
    {
        public readonly Dictionary<string, EnsemblGene> EnsemblIdToGene;
        public readonly Dictionary<string, string> EnsemblIdToSymbol;

        private EnsemblGtf(Dictionary<string, EnsemblGene> ensemblIdToGene, Dictionary<string, string> ensemblIdToSymbol)
        {
            EnsemblIdToGene   = ensemblIdToGene;
            EnsemblIdToSymbol = ensemblIdToSymbol;
        }

        public static EnsemblGtf Create(string filePath, IDictionary<string, IChromosome> refNameToChromosome)
        {
            var ensemblGenes      = LoadEnsemblGenes(GZipUtilities.GetAppropriateStreamReader(filePath), refNameToChromosome);
            var ensemblIdToGene   = ensemblGenes.GetSingleValueDict(x => x.GeneId);
            var ensemblIdToSymbol = ensemblGenes.GetKeyValueDict(x => x.GeneId, x => x.Symbol);
            return new EnsemblGtf(ensemblIdToGene, ensemblIdToSymbol);
        }

        private static EnsemblGene[] LoadEnsemblGenes(StreamReader streamReader,
            IDictionary<string, IChromosome> refNameToChromosome)
        {
            EnsemblGene[] genes;
            using (var reader = new EnsemblGtfReader(streamReader, refNameToChromosome)) genes = reader.GetGenes();
            return genes;
        }
    }
}
