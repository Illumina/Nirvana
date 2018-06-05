
using Genome;

namespace CacheUtils.Genes.DataStructures
{
    public sealed class RefSeqGene : IFlatGene<RefSeqGene>
    {
        public IChromosome Chromosome { get; }
        public int Start { get; }
        public int End { get; set; }
        private bool OnReverseStrand { get; }
        public string GeneId { get; }
        public string Symbol { get; }
        private int HgncId { get; }

        public RefSeqGene(IChromosome chromosome, int start, int end, bool onReverseStrand, string entrezGeneId,
            string symbol, int hgncId)
        {
            Chromosome      = chromosome;
            Start           = start;
            End             = end;
            OnReverseStrand = onReverseStrand;
            GeneId          = entrezGeneId;
            Symbol          = symbol;
            HgncId          = hgncId;
        }

        public RefSeqGene Clone() => new RefSeqGene(Chromosome, Start, End, OnReverseStrand, GeneId, Symbol, HgncId);
    }
}
