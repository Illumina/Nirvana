using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.TranscriptAnnotation
{
    public sealed class BreakPointTranscript
    {
        public readonly ITranscript Transcript;
        public readonly int GenomicPosition;
        public readonly int RegionIndex;

        public BreakPointTranscript(ITranscript transcript, int genomicPosition, int regionIndex)
        {
            Transcript      = transcript;
            GenomicPosition = genomicPosition;
            RegionIndex     = regionIndex;
        }
    }
}