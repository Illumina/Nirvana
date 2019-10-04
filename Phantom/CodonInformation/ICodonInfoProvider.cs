using Genome;

namespace Phantom.CodonInformation
{
    public interface ICodonInfoProvider
    {
        int GetLongestFunctionBlockDistance(IChromosomeInterval chrInterval);
    }
}