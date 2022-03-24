using Genome;

namespace VariantAnnotation.Interface
{
    public interface IVariantIdCreator
    {
        string Create(ISequence sequence, VariantCategory category, string svType, Chromosome chromosome, int start, int end, string refAllele,
            string altAllele, string repeatUnit);

        (int Start, string RefAllele, string AltAllele) Normalize(ISequence sequence, int start, string refAllele, string altAllele);
    }
}