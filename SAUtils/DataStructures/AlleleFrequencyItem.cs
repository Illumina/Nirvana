using System;
using Genome;
using VariantAnnotation.Interface.SA;

namespace SAUtils.DataStructures
{
    public sealed class AlleleFrequencyItem:ISupplementaryDataItem
    {
        public IChromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }

        public readonly double AltFrequency;

        public AlleleFrequencyItem(IChromosome chromosome, int position, string refAllele, string altAllele, double altFrequency)
        {
            Chromosome      = chromosome;
            Position        = position;
            AltFrequency    = altFrequency;
            RefAllele = refAllele;
            AltAllele = altAllele;
        }

        public string GetJsonString()
        {
            throw new NotImplementedException();
        }
        
    }
}