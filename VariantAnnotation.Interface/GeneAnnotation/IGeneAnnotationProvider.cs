using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.Interface.GeneAnnotation
{
    public interface IGeneAnnotationProvider:IProvider
    {
        IAnnotatedGene Annotate(string geneName);
    }
}