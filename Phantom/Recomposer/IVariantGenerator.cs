using System.Collections.Generic;
using VariantAnnotation.Interface.Positions;

namespace Phantom.Recomposer
{
    public interface IVariantGenerator
    {
        IEnumerable<ISimplePosition> Recompose(List<ISimplePosition> simplePositions, List<int> functionBlockRanges);
    }
}