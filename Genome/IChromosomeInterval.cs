using Intervals;

namespace Genome
{
    public interface IChromosomeInterval : IInterval
    {
        IChromosome Chromosome { get; }
    }
}