using Genome;
using VariantAnnotation.Interface.SA;

namespace SAUtils.Revel
{
    public sealed class RevelItem : ISupplementaryDataItem
    {
        public IChromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }
        private readonly string _score;
        
        public RevelItem(IChromosome chromosome, int position, string refAllele, string altAllele, string score)
        {
            Chromosome = chromosome;
            Position   = position;
            RefAllele  = refAllele;
            AltAllele  = altAllele;
            _score     = score;
        }
        
        public string GetJsonString() => $"\"score\":{_score}";
    }
}