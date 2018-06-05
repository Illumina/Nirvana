using Genome;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.Interface.Plugins
{
    public interface IPlugin : IProvider
    {
        void Annotate(IAnnotatedPosition annotatedPosition, ISequence referenceSequence);
    }    
}