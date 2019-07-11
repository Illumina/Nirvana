using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.TranscriptAnnotation
{
    public static class RohTranscriptAnnotator
    {
        public static IAnnotatedTranscript GetAnnotatedTranscript(ITranscript transcript)
        {
            return transcript.IsCanonical ? new AnnotatedTranscript(transcript, null, null, null, null, null, null, null, null, null,
                null, null, null) : null;
        }
    }
}