using VariantAnnotation.AnnotatedPositions.Consequence;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.TranscriptAnnotation
{
    public static class FlankingTranscriptAnnotator
    {
        public static IAnnotatedTranscript GetAnnotatedTranscript(int variantEnd,ITranscript Transcript)
        {
            var isDownStream = variantEnd < Transcript.Start == Transcript.Gene.OnReverseStrand;

            var consequence = new Consequences();

            consequence.DetermineFlankingVariantEffects(isDownStream);

            return new AnnotatedTranscript(Transcript,null,null,null,null,null,null,null,null,null, consequence.GetConsequences(), null, null);
        }
    }
}