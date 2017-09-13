using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.Interface.Providers
{
    public interface IRefMinorProvider
    {
        bool IsReferenceMinor(IChromosome chromosome, int pos);

        string GetGlobalMajorAlleleForRefMinor(IChromosome chromosome, int pos);
    }
}