using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Pools;

namespace VariantAnnotation.TranscriptAnnotation
{
    public static class RohTranscriptAnnotator
    {
        public static IAnnotatedTranscript GetAnnotatedTranscript(ITranscript transcript)
        {
            return transcript.IsCanonical ? AnnotatedTranscriptPool.Get(transcript, null, null, null, null, null, null, null, null, null,
                null, null) : null;
        }
    }
}