using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.Interface
{
    public interface IPluginAnnotator
    {
        GenomeAssembly GenomeAssembly { get; }
        IDataSourceVersion DataSourceVersion { get; }
        void Annotate(IAnnotatedPosition annotatedPosition, ISequence sequence);
    }
}