using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace Vcf
{
    public sealed class Position : IPosition
    {
        public IChromosome Chromosome { get; }
        public int Start { get; }
        public int End { get; }        
        public string RefAllele { get; }
        public string[] AltAlleles { get; }
        public double? Quality { get; }
        public string[] Filters { get; }
        public IVariant[] Variants { get; }
        public ISample[] Samples { get; }
        public IInfoData InfoData { get; }
        public string[] VcfFields { get; }

        public Position(IChromosome chromosome, int start, int end, string refAllele, string[] altAlleles,
            double? quality, string[] filters, IVariant[] variants, ISample[] samples, IInfoData infoData,
            string[] vcfFields)
        {
            Chromosome = chromosome;
            Start      = start;
            End        = end;
            RefAllele  = refAllele;
            AltAlleles = altAlleles;
            Quality    = quality;
            Filters    = filters;
            Variants   = variants;
            Samples    = samples;
            InfoData   = infoData;
            VcfFields  = vcfFields;
        }
    }
}
