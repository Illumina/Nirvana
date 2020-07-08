using System.Collections.Generic;
using VariantAnnotation.AnnotatedPositions.Consequence;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.TranscriptAnnotation
{
    public static class FlankingTranscriptAnnotator
    {
        public static IAnnotatedTranscript GetAnnotatedTranscript(int variantEnd, ITranscript transcript)
        {
            bool                 isDownStream = variantEnd < transcript.Start == transcript.Gene.OnReverseStrand;
            List<ConsequenceTag> consequences = Consequences.DetermineFlankingVariantEffects(isDownStream);
            return new AnnotatedTranscript(transcript, null, null, null, null, null, null, null, null, null, consequences, false);
        }
    }
}