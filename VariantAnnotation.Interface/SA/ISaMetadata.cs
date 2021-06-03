using Genome;
using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.Interface.SA
{
    public interface ISaMetadata
    {
        GenomeAssembly     Assembly { get; }
        IDataSourceVersion Version  { get; }
        string             JsonKey  { get; }
    }
}