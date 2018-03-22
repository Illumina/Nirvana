using System.Collections.Generic;
using CacheUtils.Genes.DataStructures;
using CacheUtils.Genes.IO;
using CacheUtils.Genes.Utilities;
using Compression.Utilities;

namespace CacheUtils.Genes.DataStores
{
    public sealed class GeneInfoData
    {
        public readonly Dictionary<string, string> EntrezGeneIdToSymbol;

        private GeneInfoData(Dictionary<string, string> entrezGeneIdToSymbol)
        {
            EntrezGeneIdToSymbol = entrezGeneIdToSymbol;
        }

        public static GeneInfoData Create(string filePath)
        {
            var entrezGeneIdToSymbol = LoadGeneInfoGenes(filePath)
                .GetKeyValueDict(x => x.EntrezGeneId, x => x.Symbol);
            return new GeneInfoData(entrezGeneIdToSymbol);
        }

        private static IEnumerable<GeneInfo> LoadGeneInfoGenes(string filePath)
        {
            GeneInfo[] genes;
            using (var streamReader = GZipUtilities.GetAppropriateStreamReader(filePath))
            using (var reader = new GeneInfoReader(streamReader)) genes = reader.GetGenes();
            return genes;
        }
    }
}
