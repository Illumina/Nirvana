
using Genome;

namespace CacheUtils.Genes.DataStructures
{
    public interface IFlatGene<out T>
    {
        Chromosome Chromosome { get; }
        int Start { get; }
        int End { get; set; }
        T Clone();
    }
}
