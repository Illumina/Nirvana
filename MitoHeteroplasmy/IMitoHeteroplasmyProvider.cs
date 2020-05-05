using Genome;

namespace MitoHeteroplasmy
{
    public interface IMitoHeteroplasmyProvider
    {
        double?[] GetVrfPercentiles(string genotype, IChromosome chrome, int position, string[] altAllele, double[] vrfs);
    }
}