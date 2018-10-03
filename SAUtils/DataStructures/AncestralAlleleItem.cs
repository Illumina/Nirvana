using Genome;
using VariantAnnotation.Interface.SA;

namespace SAUtils.DataStructures
{
    public sealed class AncestralAlleleItem: ISupplementaryDataItem
    {
        public IChromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }
        public readonly string AncestralAllele;

        public AncestralAlleleItem(IChromosome chromosome, int position, string refAllele, string altAllele, string ancestralAllele)
        {
            Chromosome = chromosome;
            Position = position;
            RefAllele = refAllele;
            AltAllele = altAllele;
            AncestralAllele = ancestralAllele;
        }

        public string GetJsonString()
        {
            return $"\"{AncestralAllele}\"";
        }
    }
}