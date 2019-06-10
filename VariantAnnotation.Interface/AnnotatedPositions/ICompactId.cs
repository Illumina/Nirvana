using IO;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface ICompactId : ISerializable
    {
        bool IsEmpty();
        bool IsPredictedTranscript();
        string WithVersion { get; }
        string WithoutVersion { get; }
    }
}