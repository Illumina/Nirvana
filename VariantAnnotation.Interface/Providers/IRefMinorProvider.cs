
using Genome;

namespace VariantAnnotation.Interface.Providers
{
    public interface IRefMinorProvider
    {
        string GetGlobalMajorAllele(IChromosome chromosome, int pos);
    }
}