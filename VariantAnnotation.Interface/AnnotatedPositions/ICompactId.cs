using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface ICompactId : ISerializable
    {
        bool IsEmpty();
        string WithVersion { get; }
        string WithoutVersion { get; }
    }
}