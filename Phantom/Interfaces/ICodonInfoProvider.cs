using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;

namespace Phantom.Interfaces
{
    public interface ICodonInfoProvider
    {
        int GetFunctionBlockRanges(IInterval interval);
    }
}