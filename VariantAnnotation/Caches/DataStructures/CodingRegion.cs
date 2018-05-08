using IO;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.Caches.DataStructures
{
    public sealed class CodingRegion : ICodingRegion
    {
        public int Start { get; }
        public int End { get; }
        public int CdnaStart { get; }
        public int CdnaEnd { get; }
        public int Length { get; }

        public CodingRegion(int start, int end, int cdnaStart, int cdnaEnd, int length)
        {
            Start     = start;
            End       = end;
            CdnaStart = cdnaStart;
            CdnaEnd   = cdnaEnd;
            Length    = length;
        }

        public static ICodingRegion Read(BufferedBinaryReader reader)
        {
            int genomicStart = reader.ReadOptInt32();
            int genomicEnd   = reader.ReadOptInt32();
            int cdnaStart    = reader.ReadOptInt32();
            int cdnaEnd      = reader.ReadOptInt32();
            int length       = reader.ReadOptInt32();

            return new CodingRegion(genomicStart, genomicEnd, cdnaStart, cdnaEnd, length);
        }

        public void Write(IExtendedBinaryWriter writer)
        {
            writer.WriteOpt(Start);
            writer.WriteOpt(End);
            writer.WriteOpt(CdnaStart);
            writer.WriteOpt(CdnaEnd);
            writer.WriteOpt(Length);
        }
    }
}
