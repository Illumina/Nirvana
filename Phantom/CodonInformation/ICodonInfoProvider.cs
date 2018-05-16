using Genome;

namespace Phantom.CodonInformation
{
    public interface ICodonInfoProvider
    {
        int[] GetFunctionBlockDistances(IChromosomeInterval chrInterval);

        int GetLongestFunctionBlockDistance(IChromosomeInterval chrInterval);
    }
}