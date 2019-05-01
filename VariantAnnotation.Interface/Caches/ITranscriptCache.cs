using Intervals;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.Interface.Caches
{
    public interface ITranscriptCache : IProvider
    {
        IIntervalForest<ITranscript> TranscriptIntervalForest { get; }
        IIntervalForest<IRegulatoryRegion> RegulatoryIntervalForest { get; }
    }
}