using System.Collections.Generic;
using System.IO;
using VariantAnnotation.Interface.Phantom;
using VariantAnnotation.Interface.Positions;

namespace Phantom.Workers
{
    public sealed class NullRecomposer : IRecomposer
    {
        public IEnumerable<ISimplePosition> ProcessSimplePosition(ISimplePosition simplePosition)
        {
            return new[] {simplePosition};
        }
    }
}