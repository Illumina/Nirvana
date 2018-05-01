using Intervals;
using IO;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface ITranscriptRegion : IInterval, ISerializable
    {
        TranscriptRegionType Type { get; }
        ushort Id { get; }
        int CdnaStart { get; }
        int CdnaEnd { get; }
    }

    public enum TranscriptRegionType : byte
    {
        Exon,
        Gap,
        Intron
    }
}
