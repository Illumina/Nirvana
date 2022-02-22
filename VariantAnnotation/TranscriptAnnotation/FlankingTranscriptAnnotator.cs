using Cache.Data;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.Consequence;

namespace VariantAnnotation.TranscriptAnnotation
{
    public static class FlankingTranscriptAnnotator
    {
        public static AnnotatedTranscript GetAnnotatedTranscript(int variantEnd, Transcript transcript)
        {
            var isDownStream = variantEnd < transcript.Start == transcript.Gene.OnReverseStrand;
            var consequence  = new Consequences();

            consequence.DetermineFlankingVariantEffects(isDownStream);
            return new AnnotatedTranscript(transcript, null, null, null, null, null, null, null,
                consequence.GetConsequences(), null, false);
        }
    }
}