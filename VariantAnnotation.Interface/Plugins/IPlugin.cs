using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.Interface.Plugins
{
    public interface IPlugin : IProvider
    {
        void Annotate(IAnnotatedPosition annotatedPosition, ISequence referenceSequence);
    }
    
}