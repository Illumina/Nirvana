using System;
using IO;

namespace VariantAnnotation.NSA
{
    public sealed class Chunk
    {
        private readonly int _start;
        private readonly int _end;
        public readonly long FilePosition;
        public readonly int Length;

        public Chunk(int start, int end, long filePosition, int length)
        {
            _start       = start;
            _end         = end;
            FilePosition = filePosition;
            Length       = length;
        }

        [Obsolete("Use a factory method instead of an extra constructor.")]
        public Chunk(ExtendedBinaryReader reader)
        {
            _start       = reader.ReadOptInt32();
            _end         = reader.ReadOptInt32();
            FilePosition = reader.ReadOptInt64();
            Length       = reader.ReadOptInt32();
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteOpt(_start);
            writer.WriteOpt(_end);
            writer.WriteOpt(FilePosition);
            writer.WriteOpt(Length);
        }

        public int CompareTo(int position)
        {
            if (_start <= position && position <= _end) return 0;
            return _start.CompareTo(position);
        }
    }
}