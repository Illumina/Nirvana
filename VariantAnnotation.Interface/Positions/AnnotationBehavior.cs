namespace VariantAnnotation.Interface.Positions
{
    public sealed class AnnotationBehavior
    {
        public readonly bool NeedSaPosition;
        public readonly bool NeedSaInterval;
        public readonly bool ReducedTranscriptAnnotation;
        public readonly bool NeedFlankingTranscript;
        public readonly bool ReportOverlappingGenes;
        public readonly bool StructuralVariantConsequence;
        public readonly bool NeedVerboseTranscripts;

        public AnnotationBehavior(bool needSaPosition, bool needSaInterval, bool reducedTranscriptAnnotation,
            bool needFlankingTranscript, bool reportOverlappingGenes, bool structuralVariantConsequence,
            bool needVerboseTranscript = false)
        {
            NeedSaPosition               = needSaPosition;
            NeedSaInterval               = needSaInterval;
            ReducedTranscriptAnnotation  = reducedTranscriptAnnotation;
            NeedFlankingTranscript       = needFlankingTranscript;
            ReportOverlappingGenes       = reportOverlappingGenes;
            StructuralVariantConsequence = structuralVariantConsequence;
            NeedVerboseTranscripts       = needVerboseTranscript;
        }
    }
}