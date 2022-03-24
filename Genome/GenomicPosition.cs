namespace Genome
{
    public struct GenomicPosition
    {
        public readonly Chromosome Chromosome;
        public readonly int Position;

        public GenomicPosition(Chromosome chromosome, int position)
        {
            Chromosome = chromosome;
            Position = position;
        }
    }
}