using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.Intervals
{
    public struct Interval : IInterval, ISerializable
    {
        public int Start { get; }
        public int End { get; }

        public Interval(int start, int end)
        {
            Start = start;
            End   = end;
        }

        public static IInterval Read(IExtendedBinaryReader reader)
        {
            int start = reader.ReadOptInt32();
            int end   = reader.ReadOptInt32();
            return new Interval(start, end);
        }

        public void Write(IExtendedBinaryWriter writer)
        {
            writer.WriteOpt(Start);
            writer.WriteOpt(End);
        }
    }
}