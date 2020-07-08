using Genome;

namespace VariantAnnotation.Interface.Providers
{
    public interface IMitoHeteroplasmyProvider : IProvider
    {
        double?[] GetVrfPercentiles(IChromosome chrome, int position, string[] altAllele, double[] vrfs);
    }
}