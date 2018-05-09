using Genome;

namespace Phantom.CodonInformation
{
    public interface ICodonBlock : IChromosomeInterval
    {
        int StartPhase { get; }
        bool IsSpliced { get; }
        int? MidPositionInSplicedCodon { get; }
    }
}