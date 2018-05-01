using Genome;
using IO;
using VariantAnnotation.Interface.Caches;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IRegulatoryRegion : IChromosomeInterval, ISerializable
    {
        ICompactId Id { get; }
        RegulatoryRegionType Type { get; }
    }
}