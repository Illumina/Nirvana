using System.Collections.Generic;
using VariantAnnotation.Interface.Positions;

namespace Phantom.Recomposer
{
    public interface IBufferedPositions
    {
        List<ISimplePosition> SimplePositions { get; }
        List<bool> Recomposable { get; }
        List<int> FunctionBlockRanges { get; }

        List<ISimplePosition> GetRecomposablePositions();
    }

}