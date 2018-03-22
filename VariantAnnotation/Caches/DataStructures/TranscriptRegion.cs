using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.IO;

namespace VariantAnnotation.Caches.DataStructures
{
    public sealed class TranscriptRegion : ITranscriptRegion
    {
        public TranscriptRegionType Type { get; }
        public ushort Id { get; }
        public int Start { get; }
        public int End { get; }
        public int CdnaStart { get; }
        public int CdnaEnd { get; }

        public TranscriptRegion(TranscriptRegionType type, ushort id, int start, int end, int cdnaStart, int cdnaEnd)
        {
            Type      = type;
            Id        = id;
            Start     = start;
            End       = end;
            CdnaStart = cdnaStart;
            CdnaEnd   = cdnaEnd;
        }

        public static ITranscriptRegion Read(ExtendedBinaryReader reader)
        {
            TranscriptRegionType type = (TranscriptRegionType)reader.ReadByte();
            ushort id                 = reader.ReadOptUInt16();
            int genomicStart          = reader.ReadOptInt32();
            int genomicEnd            = reader.ReadOptInt32();

            int cdnaStart = reader.ReadOptInt32();
            int cdnaEnd   = reader.ReadOptInt32();

            return new TranscriptRegion(type, id, genomicStart, genomicEnd, cdnaStart, cdnaEnd);
        }

        public void Write(IExtendedBinaryWriter writer)
        {
            writer.Write((byte)Type);
            writer.WriteOpt(Id);
            writer.WriteOpt(Start);
            writer.WriteOpt(End);
            writer.WriteOpt(CdnaStart);
            writer.WriteOpt(CdnaEnd);
        }
    }
}
