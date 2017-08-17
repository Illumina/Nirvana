using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.Interface.Caches
{
    public interface ITranscriptCache : IProvider
    {
        ITranscript[] GetOverlappingFlankingTranscripts(IChromosomeInterval interval);
        ITranscript[] GetOverlappingTranscripts(IChromosome chromosome, int start, int end);
        IRegulatoryRegion[] GetOverlappingRegulatoryRegions(IChromosomeInterval interval);
    }
}