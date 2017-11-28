using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IRnaEdit : IInterval, ISerializable
    {
        string Bases { get; }
    }
}
