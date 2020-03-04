using Genome;

namespace ReferenceSequence.Creation
{
    internal sealed class FastaSequence
    {
        public readonly IChromosome Chromosome;
        public readonly string Bases;

        internal FastaSequence(IChromosome chromosome, string bases)
        {
            Chromosome = chromosome;
            Bases      = bases;
        }
    }
}
