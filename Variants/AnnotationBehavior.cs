namespace Variants
{
    public sealed class AnnotationBehavior
    {
        public readonly bool CanonicalTranscriptOnly;
        public readonly bool NeedFlankingTranscripts;
        public readonly bool NeedRegulatoryRegions;
        public readonly bool NeedSaInterval;
        public readonly bool NeedSaPosition;

        public readonly bool MinimalTranscriptAnnotation;
        public readonly bool ReducedTranscriptAnnotation;

        public static readonly AnnotationBehavior SmallVariants         = new AnnotationBehavior(false, false, true, true, false, true, false);
        public static readonly AnnotationBehavior NonInformativeAlleles = new AnnotationBehavior(false, true, false, false, false, false, false);
        public static readonly AnnotationBehavior StructuralVariants    = new AnnotationBehavior(false, false, false, true, true, false, true);
        public static readonly AnnotationBehavior RepeatExpansions      = new AnnotationBehavior(false, false, false, true, false, false, true);
        public static readonly AnnotationBehavior RunsOfHomozygosity    = new AnnotationBehavior(true, false, false, false, false, false, true);

        private AnnotationBehavior(bool canonicalTranscriptOnly, bool minimalTranscriptAnnotation,
            bool needFlankingTranscripts, bool needRegulatoryRegions, bool needSaInterval, bool needSaPosition,
            bool reducedTranscriptAnnotation)
        {
            CanonicalTranscriptOnly     = canonicalTranscriptOnly;
            MinimalTranscriptAnnotation = minimalTranscriptAnnotation;
            NeedFlankingTranscripts     = needFlankingTranscripts;
            NeedRegulatoryRegions       = needRegulatoryRegions;
            NeedSaInterval              = needSaInterval;
            NeedSaPosition              = needSaPosition;
            ReducedTranscriptAnnotation = reducedTranscriptAnnotation;
        }
    }
}