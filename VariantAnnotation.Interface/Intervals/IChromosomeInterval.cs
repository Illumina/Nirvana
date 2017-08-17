using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.Interface.Intervals
{
    public interface IChromosomeInterval : IInterval
    {
        IChromosome Chromosome { get; }
    }
}