using Intervals;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.Interface.Providers
{
    public interface ITranscriptAnnotationProvider : IAnnotationProvider
    {
        IntervalArray<ITranscript>[] TranscriptIntervalArrays { get; }
        ushort VepVersion { get; }
    }
}
