using Intervals;
using IO;

namespace VariantAnnotation.IO
{
    public static class IntervalExtensions
    {
        public static IInterval Read(IBufferedBinaryReader reader)
        {
            int start = reader.ReadOptInt32();
            int end   = reader.ReadOptInt32();
            return new Interval(start, end);
        }

        public static void Write(this IInterval interval, IExtendedBinaryWriter writer)
        {
            writer.WriteOpt(interval.Start);
            writer.WriteOpt(interval.End);
        }
    }
}