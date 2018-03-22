using System.Collections.Generic;
using VariantAnnotation.Interface.Positions;

namespace Phantom.Interfaces
{
    public interface IBufferedPositions
    {
        List<ISimplePosition> SimplePositions { get; }
        List<bool> Recomposable { get; }
        List<int> FunctionBlockRanges { get; }

        List<ISimplePosition> GetRecomposablePositions();
    }

}