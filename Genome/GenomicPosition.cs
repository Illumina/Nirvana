namespace Genome
{
    public struct GenomicPosition
    {
        public readonly IChromosome Chromosome;
        public readonly int Position;

        public GenomicPosition(IChromosome chromosome, int position)
        {
            Chromosome = chromosome;
            Position = position;
        }
    }
}