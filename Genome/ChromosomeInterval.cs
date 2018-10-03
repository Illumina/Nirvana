using Intervals;

namespace Genome
{
    public sealed class ChromosomeInterval : IChromosomeInterval
    {
        public IChromosome Chromosome { get; }
        public int Start { get; }
        public int End { get; }

        public ChromosomeInterval(IChromosome chromosome, int start, int end)
        {
            Chromosome = chromosome;
            Start      = start;
            End        = end;
        }

        public bool Overlaps(IChromosome chrom, int start, int end)
        {
            return Chromosome.Index == chrom.Index && this.Overlaps(start, end);
        }
    }
}
