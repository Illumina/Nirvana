using Genome;
using Intervals;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;

namespace VariantAnnotation.Caches
{
    public static class TranscriptIntervalForestExtensions
    {
        public static ITranscript[] GetAllFlankingValues(this IIntervalForest<ITranscript> transcriptIntervalForest,
            IChromosomeInterval interval) => transcriptIntervalForest.GetAllOverlappingValues(interval.Chromosome.Index,
            interval.Start - OverlapBehavior.FlankingLength, interval.End + OverlapBehavior.FlankingLength);
    }
}
