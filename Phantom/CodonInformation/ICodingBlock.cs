using Intervals;

namespace Phantom.CodonInformation
{
    public interface ICodingBlock : IInterval
    {
        byte StartPhase { get; }
    }
}