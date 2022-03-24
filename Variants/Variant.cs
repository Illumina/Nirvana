using Genome;

namespace Variants
{
    public sealed class Variant : IVariant
    {
        public Chromosome        Chromosome          { get; private set; }
        public int                Start               { get; private set;}
        public int                End                 { get; private set;}
        public string             RefAllele           { get; private set;}
        public string             AltAllele           { get; private set;}
        public VariantType        Type                { get; private set;}
        public string             VariantId           { get; private set;}
        public bool               IsRefMinor          { get; private set;}
        public bool               IsRecomposed        { get; private set;}
        public bool               IsDecomposed        { get; private set;}
        public string[]           LinkedVids          { get; private set;}
        public AnnotationBehavior Behavior            { get; private set;}
        public bool               IsStructuralVariant { get; private set;}
        
        public void Initialize(Chromosome chromosome, int start, int end, string refAllele, string altAllele,
            VariantType variantType, string variantId, bool isRefMinor, bool isDecomposed, bool isRecomposed,
            string[] linkedVids, AnnotationBehavior behavior, bool isStructuralVariant)
        {
            Chromosome          = chromosome;
            Start               = start;
            End                 = end;
            RefAllele           = refAllele;
            AltAllele           = altAllele;
            Type                = variantType;
            VariantId           = variantId;
            IsRefMinor          = isRefMinor;
            IsRecomposed        = isRecomposed;
            IsDecomposed        = isDecomposed;
            LinkedVids          = linkedVids;
            Behavior            = behavior;
            IsStructuralVariant = isStructuralVariant;
        }
    }
}