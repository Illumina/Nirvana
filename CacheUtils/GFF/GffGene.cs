namespace CacheUtils.GFF
{
    public sealed class GffGene : IGffGene
    {
        public int Start { get; }
        public int End { get; }
        public string Id { get; }
        public string EntrezGeneId { get; }
        public string EnsemblGeneId { get; }
        public string Symbol { get; }

        public GffGene(int start, int end, string id, string entrezGeneId, string ensemblGeneId, string symbol)
        {
            Start         = start;
            End           = end;
            Id            = id;
            EntrezGeneId  = entrezGeneId;
            EnsemblGeneId = ensemblGeneId;
            Symbol        = symbol;
        }
    }
}
