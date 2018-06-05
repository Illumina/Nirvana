using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.Genes.DataStructures;
using CacheUtils.Genes.IO;
using CacheUtils.Genes.Utilities;
using Compression.Utilities;
using Genome;

namespace CacheUtils.Genes.DataStores
{
    public sealed class RefSeqGff
    {
        public readonly Dictionary<string, List<RefSeqGene>> EntrezGeneIdToGene;
        public readonly Dictionary<string, string> EntrezGeneIdToSymbol;

        private RefSeqGff(Dictionary<string, List<RefSeqGene>> entrezGeneIdToGene, Dictionary<string, string> entrezGeneIdToSymbol)
        {
            EntrezGeneIdToGene   = entrezGeneIdToGene;
            EntrezGeneIdToSymbol = entrezGeneIdToSymbol;
        }

        public static RefSeqGff Create(string gcfGffPath, string refGffPath, IDictionary<string, IChromosome> accessionToChromosome)
        {
            var refSeqGenes = LoadRefSeqGffGenes(GZipUtilities.GetAppropriateStreamReader(gcfGffPath),
                GZipUtilities.GetAppropriateStreamReader(refGffPath), accessionToChromosome);

            var entrezGeneIdToGene = refSeqGenes
                    .GetMultiValueDict(x => x.GeneId)
                    .FlattenGeneList()
                    .GetMultiValueDict(x => x.GeneId);

            var entrezGeneIdToSymbol = refSeqGenes.GetKeyValueDict(x => x.GeneId, x => x.Symbol);

            return new RefSeqGff(entrezGeneIdToGene, entrezGeneIdToSymbol);
        }

        private static List<RefSeqGene> LoadRefSeqGffGenes(StreamReader gcfGffReader, StreamReader refGffReader, IDictionary<string, IChromosome> accessionToChromosome)
        {
            var refSeqGenes = new List<RefSeqGene>();

            LoadRefSeqGff(gcfGffReader, refSeqGenes, accessionToChromosome);
            LoadRefSeqGff(refGffReader, refSeqGenes, accessionToChromosome);

            return refSeqGenes.OrderBy(x => x.Chromosome.Index).ThenBy(x => x.Start).ThenBy(x => x.End).ToList();
        }

        private static void LoadRefSeqGff(StreamReader streamReader, List<RefSeqGene> refSeqGenes, IDictionary<string, IChromosome> accessionToChromosome)
        {
            using (var reader = new RefSeqGffReader(streamReader, accessionToChromosome))
            {
                reader.AddGenes(refSeqGenes);
            }
        }
    }
}
