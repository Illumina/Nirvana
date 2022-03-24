using Intervals;

namespace Genome
{
    public interface IChromosomeInterval : IInterval
    {
        Chromosome Chromosome { get; }
    }
}