using System.Collections.Generic;
using VariantAnnotation.Interface.Positions;

namespace VariantAnnotation.Interface.Phantom
{
    public interface IRecomposer
    {
        IEnumerable<ISimplePosition> ProcessSimplePosition(ISimplePosition simplePosition);
    }
}