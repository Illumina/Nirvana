using System;
using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.Interface.GeneAnnotation
{
    public interface IGeneAnnotationProvider:IProvider, IDisposable
    {
        string Annotate(string geneName);
    }
}