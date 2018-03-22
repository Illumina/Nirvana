using System;
using VariantAnnotation.Interface.Sequence;

namespace CacheUtils.Genes.DataStructures
{
    public sealed class EnsemblGene : IFlatGene<EnsemblGene>
    {
        public IChromosome Chromosome { get; }
        public int Start { get; }
        public int End { get; set; }
        public string GeneId { get; }
        public string Symbol { get; }

        public EnsemblGene(IChromosome chromosome, int start, int end, string geneId, string symbol)
        {
            Chromosome      = chromosome;
            Start           = start;
            End             = end;
            GeneId          = geneId;
            Symbol          = symbol;
        }

        public EnsemblGene Clone() => throw new NotImplementedException();
    }
}
