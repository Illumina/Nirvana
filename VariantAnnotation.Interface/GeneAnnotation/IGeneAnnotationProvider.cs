using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.Interface.GeneAnnotation
{
    public interface IGeneAnnotationProvider : IProvider
    {
        string Annotate(string geneName);
    }
}