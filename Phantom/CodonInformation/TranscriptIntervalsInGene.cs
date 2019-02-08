using Intervals;

namespace Phantom.CodonInformation
{
    public sealed class TranscriptIntervalsInGene
    {
        public readonly IInterval[][] Intervals;
        public readonly int NumTranscripts;

        public TranscriptIntervalsInGene(IInterval[][] intervals)
        {
            Intervals = intervals;
            NumTranscripts = intervals.Length;
        }
    }
}