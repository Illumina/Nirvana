using VariantAnnotation.Interface.Caches;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IRegulatoryRegion : IChromosomeInterval, ISerializable
    {
        ICompactId Id { get; }
        RegulatoryRegionType Type { get; }
    }
}