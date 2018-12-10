using Genome;

namespace VariantAnnotation.Interface.SA
{
    public interface ISuppIntervalItem : IChromosomeInterval
    {
        string GetJsonString();
    }
}