using System;

namespace Variants
{
    public sealed class AnnotationBehavior : IEquatable<AnnotationBehavior>
    {
        public readonly bool NeedSaPosition;
        public readonly bool NeedSaInterval;
        public readonly bool ReducedTranscriptAnnotation;
        public readonly bool NeedFlankingTranscript;
        public readonly bool StructuralVariantConsequence;
        public readonly bool CanonicalTranscriptOnly;

        public static readonly AnnotationBehavior SmallVariantBehavior      = new AnnotationBehavior(true, false, false, true, false);
        public static readonly AnnotationBehavior MinimalAnnotationBehavior = new AnnotationBehavior(false, false, false, false, false);
        public static readonly AnnotationBehavior CnvBehavior               = new AnnotationBehavior(false, true, true, false, true);
        public static readonly AnnotationBehavior RefVariantBehavior        = new AnnotationBehavior(true, false, false, true, false);
        public static readonly AnnotationBehavior RepeatExpansionBehavior   = new AnnotationBehavior(false, false, true, false, true);
        public static readonly AnnotationBehavior StructuralVariantBehavior = new AnnotationBehavior(false, true, true, false, true);
        public static readonly AnnotationBehavior RohBehavior = new AnnotationBehavior(false, false, true, false, true, true);


        public AnnotationBehavior(bool needSaPosition, bool needSaInterval, bool reducedTranscriptAnnotation,
            bool needFlankingTranscript, bool structuralVariantConsequence, 
            bool canonicalTranscriptOnly = false)
        {
            NeedSaPosition               = needSaPosition;
            NeedSaInterval               = needSaInterval;
            ReducedTranscriptAnnotation  = reducedTranscriptAnnotation;
            NeedFlankingTranscript       = needFlankingTranscript;
            StructuralVariantConsequence = structuralVariantConsequence;
            CanonicalTranscriptOnly      = canonicalTranscriptOnly;
        }

        public bool Equals(AnnotationBehavior other) =>
            NeedSaPosition               == other.NeedSaPosition              &&
            NeedSaInterval               == other.NeedSaInterval              &&
            ReducedTranscriptAnnotation  == other.ReducedTranscriptAnnotation &&
            NeedFlankingTranscript       == other.NeedFlankingTranscript      &&
            StructuralVariantConsequence == other.StructuralVariantConsequence &&
            CanonicalTranscriptOnly      == other.CanonicalTranscriptOnly;
    }
}