namespace Genome
{
    public sealed class ChromosomeInterval : IChromosomeInterval
    {
        public Chromosome Chromosome { get; }
        public int Start { get; }
        public int End { get; }

        public ChromosomeInterval(Chromosome chromosome, int start, int end)
        {
            Chromosome = chromosome;
            Start      = start;
            End        = end;
        }
    }
}
