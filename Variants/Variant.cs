using Genome;

namespace Variants
{
    public sealed class Variant : IVariant
    {
        public IChromosome Chromosome { get; }
        public int Start { get; }
        public int End { get; }
        public string RefAllele { get; }
        public string AltAllele { get; }
        public VariantType Type { get; }
        public string VariantId { get; }
        public bool IsRefMinor { get; }
        public bool IsRecomposed { get; }
        public bool IsDecomposed { get; }
        public string[] LinkedVids { get; }
	    public IBreakEnd[] BreakEnds { get; }
	    public AnnotationBehavior Behavior { get; }

        public Variant(IChromosome chromosome, int start, int end, string refAllele, string altAllele,
            VariantType variantType, string variantId, bool isRefMinor, bool isDecomposed, bool isRecomposed, string[] linkedVids, IBreakEnd[] breakEnds,
            AnnotationBehavior behavior)
        {
            Chromosome   = chromosome;
            Start        = start;
            End          = end;
            RefAllele    = refAllele;
            AltAllele    = altAllele;
            Type         = variantType;
            VariantId    = variantId;
            IsRefMinor   = isRefMinor;
            IsRecomposed = isRecomposed;
            IsDecomposed = isDecomposed;
            LinkedVids   = linkedVids;
            Behavior     = behavior;
	        BreakEnds    = breakEnds;
        }
    }
}