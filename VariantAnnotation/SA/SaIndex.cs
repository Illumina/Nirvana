using System;
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
        public int[] RefMinorPositions { get; }

        private Interval<long> _cachedInterval = IntervalArray<long>.EmptyInterval;

        /// <summary>
        /// constructor
        /// </summary>
        private SaIndex(IIntervalSearch<long> fileOffsetIntervals, int[] refMinorPositions)
        {
            _fileOffsetIntervals = fileOffsetIntervals;
            RefMinorPositions = refMinorPositions;
        }



        public long GetOffset(int position)
        {
            if (_cachedInterval.Contains(position)) return _cachedInterval.Value;

            Interval<long> interval;
            if (!_fileOffsetIntervals.GetFirstOverlappingInterval(position, position, out interval))
            {
                return -1;
            }

            return interval.Value;
        }

        public bool IsRefMinor(int position)
        {
            int index = Array.BinarySearch<int>(RefMinorPositions, position);
            return index >= 0;
        }

        public static ISaIndex Read(Stream stream)
        {
            IntervalArray<long> intervalArray;
            int[] refMinorPositions;

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

                refMinorPositions = GetRefMinor(reader);
            }

            return new SaIndex(intervalArray, refMinorPositions);
        }

        private static int[] GetRefMinor(ExtendedBinaryReader reader)
        {
            var numPositions = reader.ReadOptInt32();
            var refMinorPositions = new int[numPositions];

            int oldPosition = 0;

            for (int i = 0; i < numPositions; i++)
            {
                var deltaPosition = reader.ReadOptInt32();
                refMinorPositions[i] = oldPosition + deltaPosition;
                oldPosition = refMinorPositions[i];
            }

            return refMinorPositions;
        }

        public static void Write(IExtendedBinaryWriter writer, List<Interval<long>> intervals,
            List<int> refMinorPositions)
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

        private static void WriteRefMinor(IExtendedBinaryWriter writer, List<int> refMinorPositions)
        {
            writer.WriteOpt(refMinorPositions.Count);

            int oldPosition = 0;

            foreach (var position in refMinorPositions.OrderBy(x => x))
            {
                int deltaPosition = position - oldPosition;
                writer.WriteOpt(deltaPosition);
                oldPosition = position;
            }
        }
    }
}