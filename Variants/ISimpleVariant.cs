using Genome;
using Intervals;

namespace Variants
{
    public interface ISimpleVariant : IInterval
    {
        IChromosome Chromosome { get; }
        string RefAllele { get; }
        string AltAllele { get; }
        VariantType Type { get; }
    }
}