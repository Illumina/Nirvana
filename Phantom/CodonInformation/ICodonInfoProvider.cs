using Intervals;

namespace Phantom.CodonInformation
{
    public interface ICodonInfoProvider
    {
        int GetFunctionBlockRanges(IInterval interval);
    }
}