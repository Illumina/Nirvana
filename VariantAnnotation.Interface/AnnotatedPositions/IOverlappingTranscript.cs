using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IOverlappingTranscript : IJsonSerializer
    {
        ICompactId Id { get; }
        string GeneName { get; }
        bool IsPartionalOverlap { get; }
        bool IsCanonical { get; }
    }
}