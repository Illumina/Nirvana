using VariantAnnotation.Interface.Positions;

namespace Phantom.Recomposer
{
    public interface IPositionBuffer
    {
        BufferedPositions AddPosition(ISimplePosition simplePosition);
        BufferedPositions Purge();
    }
}