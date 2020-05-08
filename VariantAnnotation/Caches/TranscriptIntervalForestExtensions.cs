using Genome;
using Intervals;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.Caches
{
    public static class TranscriptIntervalForestExtensions
    {
        public static ITranscript[] GetAllFlankingValues(this IIntervalForest<ITranscript> transcriptIntervalForest,
            IChromosomeInterval interval) => transcriptIntervalForest.GetAllOverlappingValues(interval.Chromosome.Index,
            interval.Start - interval.Chromosome.FlankingLength, interval.End + interval.Chromosome.FlankingLength);
    }
}
