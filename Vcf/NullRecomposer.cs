using System.Collections.Generic;
using VariantAnnotation.Interface.Phantom;
using VariantAnnotation.Interface.Positions;

namespace Vcf
{
    public sealed class NullRecomposer : IRecomposer
    {
        public IEnumerable<ISimplePosition> ProcessSimplePosition(ISimplePosition simplePosition)
        {
            return new[] {simplePosition};
        }
    }
}