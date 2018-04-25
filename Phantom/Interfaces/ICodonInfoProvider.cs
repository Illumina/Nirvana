using VariantAnnotation.Interface.Intervals;

namespace Phantom.Interfaces
{
    public interface ICodonInfoProvider
    {
        int GetFunctionBlockRanges(IInterval interval);
    }
}