namespace CacheUtils.Genes.DataStructures
{
    public sealed class GeneInfo
    {
        public string Symbol { get; }
        public string EntrezGeneId { get; }

        public GeneInfo(string symbol, string entrezGeneId)
        {
            Symbol       = symbol;
            EntrezGeneId = entrezGeneId;
        }
    }
}
