using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;

namespace CacheUtils.DataDumperImport.DataStructures.Mutable
{
    public sealed class MutableTranscriptRegion : ITranscriptRegion
    {
        public int Start { get; }
        public int End { get; }
        public TranscriptRegionType Type { get; set; }
        public ushort Id { get; set; }
        public int CdnaStart { get; set; }
        public int CdnaEnd { get; set; }

        public MutableTranscriptRegion(TranscriptRegionType type, ushort id, int start, int end, int cdnaStart = -1,
            int cdnaEnd = -1)
        {
            Type      = type;
            Id        = id;
            Start     = start;
            End       = end;
            CdnaStart = cdnaStart;
            CdnaEnd   = cdnaEnd;
        }

        public void Write(IExtendedBinaryWriter writer) => throw new System.NotImplementedException();
    }
}
