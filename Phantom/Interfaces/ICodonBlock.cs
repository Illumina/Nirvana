using VariantAnnotation.Interface.Intervals;

namespace Phantom.Interfaces
{
    public interface ICodonBlock : IChromosomeInterval
    {
        int StartPhase { get; }
        bool IsSpliced { get; }
        int? MidPositionInSplicedCodon { get; }
    }
}