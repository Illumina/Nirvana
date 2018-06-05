using Intervals;

namespace Phantom.CodonInformation
{
    public sealed class PhasedIntervalArray
    {
        public readonly byte StartPhase;
        public readonly IInterval[] IntervalArray;

        public PhasedIntervalArray(byte startPhase, IInterval[] intervalArray)
        {
            StartPhase = startPhase;
            IntervalArray = intervalArray;
        }
    }
}