using Genome;

namespace Variants
{
    public sealed class SimpleVariant : ISimpleVariant
    {
        public int Start { get; }
        public int End { get; }
        public Chromosome Chromosome { get; }
        public string RefAllele { get; }
        public string AltAllele { get; }
        public VariantType Type { get; }

        public SimpleVariant(Chromosome chromosome, int start, int end, string refAllele, string altAllele, VariantType type)
        {
            Chromosome = chromosome;
            Start      = start;
            End        = end;
            RefAllele  = refAllele;
            AltAllele  = altAllele;
            Type       = type;
        }
    }
}