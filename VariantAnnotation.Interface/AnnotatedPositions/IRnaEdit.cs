using Intervals;
using IO;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IRnaEdit : IInterval, ISerializable
    {
        string Bases { get; }
    }
}
