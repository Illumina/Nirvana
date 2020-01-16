using Genome;
using VariantAnnotation.Interface.SA;

namespace SAUtils.Revel
{
    public class RevelItem : ISupplementaryDataItem
    {
        public IChromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }
        public string Score;
        
        public RevelItem(IChromosome chromosome, int position, string refAllele, string altAllele, string score)
        {
            Chromosome = chromosome;
            Position = position;
            RefAllele = refAllele;
            AltAllele = altAllele;
            Score = score;
        }
        
        public string GetJsonString() => $"\"score\":{Score}";
    }
}