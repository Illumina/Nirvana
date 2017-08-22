using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface ICdnaCoordinateMap : IInterval, ISerializable
    {
        int CdnaStart { get; }
        int CdnaEnd { get; }
        bool IsNull { get; }
    }
}