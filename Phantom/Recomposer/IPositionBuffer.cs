using Genome;
using Intervals;
using Phantom.CodonInformation;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Positions;

namespace Phantom.Recomposer
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