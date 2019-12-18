using Genome;

namespace Variants
{
    public sealed class RepeatExpansion : IVariant
    {
        public IChromosome Chromosome { get; }
        public int Start { get; }
        public int End { get; }
        public string RefAllele { get; }
        public string AltAllele { get; }
        public string VariantId { get; }

        public VariantType Type { get; }            = VariantType.short_tandem_repeat_variation;
        public bool IsRefMinor { get; }             = false;
        public bool IsRecomposed { get; }           = false;
        public bool IsDecomposed { get; }           = false;
        public string[] LinkedVids { get; }         = null;
        public AnnotationBehavior Behavior { get; } = AnnotationBehavior.RepeatExpansions;
        public bool IsStructuralVariant { get; }    = true;

        public readonly int RepeatCount;
        public readonly int? RefRepeatCount;

        public RepeatExpansion(IChromosome chromosome, int start, int end, string refAllele, string altAllele,
            string variantId, int repeatCount, int? refRepeatCount)
        {
            Chromosome     = chromosome;
            Start          = start;
            End            = end;
            RefAllele      = refAllele;
            AltAllele      = altAllele;
            VariantId      = variantId;
            RepeatCount    = repeatCount;
            RefRepeatCount = refRepeatCount;
        }
    }
}
