using IO;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;

namespace VariantAnnotation.Caches.DataStructures
{
    public sealed class RnaEdit : IRnaEdit
    {
        public int Start { get; }
        public int End { get; }
        public string Bases { get; }
        public VariantType Type { get; set; }

        public RnaEdit(int start, int end, string bases)
        {
            Start = start;
            End   = end;
            Bases = bases;
            Type = VariantType.unknown;
        }

        public static IRnaEdit Read(BufferedBinaryReader reader)
        {
            int start    = reader.ReadOptInt32();
            int end      = reader.ReadOptInt32();
            string bases = reader.ReadAsciiString();
            return new RnaEdit(start, end, bases);
        }

        public void Write(IExtendedBinaryWriter writer)
        {
            writer.WriteOpt(Start);
            writer.WriteOpt(End);
            writer.WriteOptAscii(Bases);
        }

        public int CompareTo(IRnaEdit other)
        {
            return Start.CompareTo(other.Start);
        }
    }
}
