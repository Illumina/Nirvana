using Genome;

namespace VariantAnnotation.Interface.Providers
{
    public interface IRefMinorProvider
    {
        void PreLoad(IChromosome chrom);
        string GetGlobalMajorAllele(IChromosome chromosome, int pos);
    }
}