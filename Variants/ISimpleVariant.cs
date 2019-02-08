using Genome;

namespace Variants
{
    public interface ISimpleVariant : IChromosomeInterval
    {
        string RefAllele { get; }
        string AltAllele { get; }
        VariantType Type { get; }
    }
}