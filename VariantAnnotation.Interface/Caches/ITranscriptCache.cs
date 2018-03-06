using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.Interface.Caches
{
    public interface ITranscriptCache : IProvider
    {
        ITranscript[] GetOverlappingTranscripts(IChromosomeInterval interval);
        ITranscript[] GetOverlappingTranscripts(IChromosome chromosome, int start, int end, int flankingLength = OverlapBehavior.FlankingLength);
        IRegulatoryRegion[] GetOverlappingRegulatoryRegions(IChromosomeInterval interval);
    }
}