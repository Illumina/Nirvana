using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.IO;

namespace VariantAnnotation.Caches.DataStructures
{
    public sealed class RnaEdit : IRnaEdit
    {
        public int Start { get; }
        public int End { get; }
        public string Bases { get; }

        public RnaEdit(int start, int end, string bases)
        {
            Start = start;
            End   = end;
            Bases = bases;
        }

        public static IRnaEdit Read(ExtendedBinaryReader reader)
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
    }
}
