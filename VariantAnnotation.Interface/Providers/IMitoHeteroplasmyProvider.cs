using Genome;
using Variants;

namespace VariantAnnotation.Interface.Providers
{
    public interface IMitoHeteroplasmyProvider : IProvider
    {
        double?[] GetVrfPercentiles(IVariant[] variants, double[] vrfs);
    }
}