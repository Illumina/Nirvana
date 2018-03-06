using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace VariantAnnotation.SA
{
    public class SaIndex : ISaIndex
    {
        private readonly IIntervalSearch<long> _fileOffsetIntervals;
        public (int Position, string GlobalMajorAllele)[] GlobalMajorAlleleForRefMinor { get; }

        private Interval<long> _cachedInterval = IntervalArray<long>.EmptyInterval;

        /// <summary>
        /// constructor
        /// </summary>
        private SaIndex(IIntervalSearch<long> fileOffsetIntervals, (int, string)[] globalMajorAlleleForRefMinor)
        {
            _fileOffsetIntervals = fileOffsetIntervals;
            GlobalMajorAlleleForRefMinor = globalMajorAlleleForRefMinor;
        }



        public long GetOffset(int position)
        {
            if (_cachedInterval.Contains(position)) return _cachedInterval.Value;

            if (!_fileOffsetIntervals.GetFirstOverlappingInterval(position, position, out var interval))
            {
                return -1;
            }

            return interval.Value;
        }


        public static ISaIndex Read(Stream stream)
        {
            IntervalArray<long> intervalArray;
            (int, string)[] globalMajorAlleleForRefMinors;

            using (var reader = new ExtendedBinaryReader(stream))
            {
                var numEntries = reader.ReadOptInt32();
                var intervals = new Interval<long>[numEntries];

                for (int i = 0; i < numEntries; i++)
                {
                    var begin = reader.ReadOptInt32();
                    var end = reader.ReadOptInt32();
                    var fileOffset = reader.ReadOptInt64();

                    intervals[i] = new Interval<long>(begin, end, fileOffset);
                }

                intervalArray = new IntervalArray<long>(intervals);

                globalMajorAlleleForRefMinors = GetGlobalMajorAlleleInRefMinor(reader);
            }

            return new SaIndex(intervalArray, globalMajorAlleleForRefMinors);
        }

        private static (int Position, string GlobalMajorAllele)[] GetGlobalMajorAlleleInRefMinor(IExtendedBinaryReader reader)
        {
            var numPositions = reader.ReadOptInt32();
            var globalMajorAlleleForRefMinors = new(int, string)[numPositions];
            int oldPosition = 0;

            for (int i = 0; i < numPositions; i++)
            {
                var deltaPosition = reader.ReadOptInt32();
                var refMinorPosition = oldPosition + deltaPosition;
                oldPosition = refMinorPosition;
                var globalMajorAllele = reader.ReadAsciiString();
                globalMajorAlleleForRefMinors[i] = (refMinorPosition, globalMajorAllele);

            }

            return globalMajorAlleleForRefMinors;
        }

        public static void Write(IExtendedBinaryWriter writer, List<Interval<long>> intervals,
            List<(int, string)> refMinorPositions)
        {
            writer.WriteOpt(intervals.Count);

            foreach (var interval in intervals.OrderBy(x => x.Begin))
            {
                writer.WriteOpt(interval.Begin);
                writer.WriteOpt(interval.End);
                writer.WriteOpt(interval.Value);
            }

            WriteRefMinor(writer, refMinorPositions);
        }

        private static void WriteRefMinor(IExtendedBinaryWriter writer, List<(int Position, string GlobalMajorAllele)> refMinorPositions)
        {
            writer.WriteOpt(refMinorPositions.Count);

            int oldPosition = 0;

            foreach (var globalMajorAllele in refMinorPositions.OrderBy(x => x.Position))
            {
                var position = globalMajorAllele.Position;
                int deltaPosition = position - oldPosition;
                writer.WriteOpt(deltaPosition);
                oldPosition = position;
                writer.WriteOptAscii(globalMajorAllele.GlobalMajorAllele);
            }
        }
    }
}