using Genome;

namespace VariantAnnotation.Interface.Providers
{
    public interface IRefMinorProvider
    {
        string GetGlobalMajorAllele(Chromosome chromosome, int pos);
    }
}