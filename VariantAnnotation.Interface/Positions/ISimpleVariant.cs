using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.Interface.Positions
{
    public interface ISimpleVariant : IInterval
    {
        IChromosome Chromosome { get; }
        string RefAllele { get; }
        string AltAllele { get; }
        VariantType Type { get; }
    }
}