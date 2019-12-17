using Genome;

namespace VariantAnnotation.Interface
{
    public interface IVariantIdCreator
    {
        string Create(ISequence sequence, VariantCategory category, string svType, IChromosome chromosome, int start, int end, string refAllele,
            string altAllele, string repeatUnit);
    }
}