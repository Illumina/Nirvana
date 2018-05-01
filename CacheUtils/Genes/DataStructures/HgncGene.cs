
using Genome;

namespace CacheUtils.Genes.DataStructures
{
    public sealed class HgncGene : IChromosomeInterval
    {
        public IChromosome Chromosome { get; }
        public int Start { get; set; }
        public int End { get; set; }
        public string Symbol { get; }
        public string EntrezGeneId { get; set; }
        public string EnsemblId { get; set; }
        public readonly int HgncId;

        public HgncGene(IChromosome chromosome, int start, int end, string symbol, string entrezGeneId,
            string ensemblId, int hgncId)
        {
            Chromosome   = chromosome;
            Start        = start;
            End          = end;
            Symbol       = symbol;
            EntrezGeneId = entrezGeneId;
            EnsemblId    = ensemblId;
            HgncId       = hgncId;
        }

        public HgncGene Clone() => new HgncGene(Chromosome, -1, -1, Symbol, EntrezGeneId, EnsemblId, HgncId);
    }
}
