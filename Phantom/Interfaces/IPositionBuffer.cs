using Genome;
using Intervals;
using Phantom.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Positions;

namespace Phantom.Interfaces
{
    public interface IPositionBuffer
    {
        ICodonInfoProvider CodonInfoProvider { get; }
        IChromosome CurrentChromosome { get; }
        BufferedPositions BufferedPositions { get; }
        IIntervalForest<IGene> GeneIntervalForest { get; }

        bool PositionWithinRange(ISimplePosition simplePosition);
        bool InGeneRegion(ISimplePosition simplePosition);
        void UpdateFunctionBlockRanges(ISimplePosition simplePosition);
        BufferedPositions AddPosition(ISimplePosition simplePosition);
        BufferedPositions Purge();
    }
}