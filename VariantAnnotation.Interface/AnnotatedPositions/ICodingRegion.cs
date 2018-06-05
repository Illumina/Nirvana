using Intervals;
using IO;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface ICodingRegion : IInterval, ISerializable
    {
        int CdnaStart { get; }
        int CdnaEnd { get; }
        int Length { get; }
    }
}
