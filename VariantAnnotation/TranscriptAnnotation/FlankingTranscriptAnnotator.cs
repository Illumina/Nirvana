using VariantAnnotation.AnnotatedPositions.Consequence;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.TranscriptAnnotation
{
    public static class FlankingTranscriptAnnotator
    {
        public static IAnnotatedTranscript GetAnnotatedTranscript(int variantEnd, ITranscript transcript)
        {
            var isDownStream = variantEnd < transcript.Start == transcript.Gene.OnReverseStrand;
            var consequence  = new Consequences();

            consequence.DetermineFlankingVariantEffects(isDownStream);
            return new AnnotatedTranscript(transcript, null, null, null, null, null, null, null, null, null,
                consequence.GetConsequences(), null);
        }
    }
}