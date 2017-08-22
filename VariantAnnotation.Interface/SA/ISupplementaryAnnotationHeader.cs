using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.Interface.SA
{
    public interface ISupplementaryAnnotationHeader : IProvider
    {
        string ReferenceSequenceName { get; }
        long CreationTimeTicks { get; }
        ushort DataVersion { get; }
    }
}