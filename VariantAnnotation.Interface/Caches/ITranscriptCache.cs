using Genome;
using Intervals;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.Interface.Caches
{
    public interface ITranscriptCache : IProvider
    {
        ITranscript[] GetOverlappingTranscripts(IChromosomeInterval interval);
        ITranscript[] GetOverlappingTranscripts(IChromosome chromosome, int start, int end, int flankingLength = OverlapBehavior.FlankingLength);
        IRegulatoryRegion[] GetOverlappingRegulatoryRegions(IChromosomeInterval interval);
    }
}