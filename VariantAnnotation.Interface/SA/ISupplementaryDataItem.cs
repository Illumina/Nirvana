using Genome;

namespace VariantAnnotation.Interface.SA
{
    public interface ISupplementaryDataItem
    {
        IChromosome Chromosome { get; }
        int Position { get; set; }
        string RefAllele { get; set; }
        string AltAllele { get; set; }
        string GetJsonString();
    }
}