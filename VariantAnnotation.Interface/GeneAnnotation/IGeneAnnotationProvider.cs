using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.Interface.GeneAnnotation
{
    public interface IGeneAnnotationProvider:IProvider
    {
        IGeneAnnotation Annotate(string geneName);
    }
}