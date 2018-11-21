using Genome;
using Variants;

namespace VariantAnnotation.Interface.SA
{
    public interface ISuppIntervalItem : IChromosomeInterval
    {
        VariantType VariantType { get; }
        string GetJsonString();
    }
}