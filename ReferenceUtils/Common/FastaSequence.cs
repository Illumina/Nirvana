using Genome;

namespace ReferenceUtils.Common
{
    public sealed class FastaSequence
    {
        public readonly IChromosome Chromosome;
        public readonly string Bases;

        public FastaSequence(IChromosome chromosome, string bases)
        {
            Chromosome = chromosome;
            Bases      = bases;
        }
    }
}
