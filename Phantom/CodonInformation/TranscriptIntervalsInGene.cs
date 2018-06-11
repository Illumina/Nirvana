using Intervals;

namespace Phantom.CodonInformation
{
    public class TranscriptIntervalsInGene
    {
        public IInterval[][] Intervals;
        public int NumTranscripts;

        public TranscriptIntervalsInGene(IInterval[][] intervals)
        {
            Intervals = intervals;
            NumTranscripts = intervals.Length;
        }
    }
}