using VariantAnnotation.Interface.Sequence;

namespace CacheUtils.Genes.DataStructures
{
    public interface IFlatGene<out T>
    {
        IChromosome Chromosome { get; }
        int Start { get; }
        int End { get; set; }
        T Clone();
    }
}
