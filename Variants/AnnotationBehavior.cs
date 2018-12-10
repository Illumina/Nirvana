namespace Variants
{
    public sealed class AnnotationBehavior
    {
        public readonly bool NeedSaPosition;
        public readonly bool NeedSaInterval;
        public readonly bool ReducedTranscriptAnnotation;
        public readonly bool NeedFlankingTranscript;
        public readonly bool StructuralVariantConsequence;

        public AnnotationBehavior(bool needSaPosition, bool needSaInterval, bool reducedTranscriptAnnotation,
            bool needFlankingTranscript, bool structuralVariantConsequence)
        {
            NeedSaPosition               = needSaPosition;
            NeedSaInterval               = needSaInterval;
            ReducedTranscriptAnnotation  = reducedTranscriptAnnotation;
            NeedFlankingTranscript       = needFlankingTranscript;
            StructuralVariantConsequence = structuralVariantConsequence;
        }
    }
}