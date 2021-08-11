using System.Collections.Generic;
using VariantAnnotation.AnnotatedPositions.Consequence;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Pools;

namespace VariantAnnotation.TranscriptAnnotation
{
    public static class FlankingTranscriptAnnotator
    {
        public static IAnnotatedTranscript GetAnnotatedTranscript(int variantEnd, ITranscript transcript)
        {
            bool                 isDownStream = variantEnd < transcript.Start == transcript.Gene.OnReverseStrand;
            List<ConsequenceTag> consequences = Consequences.DetermineFlankingVariantEffects(isDownStream);
            return AnnotatedTranscriptPool.Get(transcript, null, null, null, null, null, null, null, null, null, consequences, false);
        }
    }
}