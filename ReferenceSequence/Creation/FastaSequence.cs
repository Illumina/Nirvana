using Genome;

namespace ReferenceSequence.Creation
{
    internal sealed class FastaSequence
    {
        public readonly Chromosome Chromosome;
        public readonly string Bases;

        internal FastaSequence(Chromosome chromosome, string bases)
        {
            Chromosome = chromosome;
            Bases      = bases;
        }
    }
}
