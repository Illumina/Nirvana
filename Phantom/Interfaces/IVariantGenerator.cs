using System.Collections.Generic;
using Phantom.DataStructures;
using VariantAnnotation.Interface.Positions;

namespace Phantom.Interfaces
{
    public interface IVariantGenerator
    {
        IEnumerable<ISimplePosition> Recompose(List<ISimplePosition> simaplePositions, List<int> functionBlockRanges);
    }
}