using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IGene : IChromosomeInterval, ISerializable
    {
        bool OnReverseStrand { get; }
        string Symbol { get; }
        ICompactId EntrezGeneId { get; }
        ICompactId EnsemblId { get; }
        int HgncId { get; }
    }
}