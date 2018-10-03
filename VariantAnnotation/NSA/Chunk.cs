using IO;

namespace VariantAnnotation.NSA
{
    public sealed class Chunk
    {
        public readonly int Start;
        public readonly int End;
        public readonly long FilePosition;
        public readonly int Length;

        public Chunk(int start, int end, long filePosition, int length)
        {
            Start = start;
            End = end;
            FilePosition = filePosition;
            Length = length;
        }

        public Chunk(ExtendedBinaryReader reader)
        {
            Start = reader.ReadOptInt32();
            End = reader.ReadOptInt32();
            FilePosition = reader.ReadOptInt64();
            Length = reader.ReadOptInt32();
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteOpt(Start);
            writer.WriteOpt(End);
            writer.WriteOpt(FilePosition);
            writer.WriteOpt(Length);
        }

        public int CompareTo(int position)
        {
            if (Start <= position && position <= End) return 0;
            return Start.CompareTo(position);
        }
    }
}