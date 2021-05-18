using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.GeneFusions.Calling
{
    public sealed record BreakPointTranscript(ITranscript Transcript, int GenomicPosition, int RegionIndex);
}