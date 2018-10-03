using System.Collections.Generic;
using System.Linq;
using Intervals;
using IO;

namespace VariantAnnotation.NSA
{
    public sealed class IntervalChromIndex
    {
        // Each SV is represented as an interval. For each SV, we need to store the beginFileOffset and endFileOffset. We store them as an interval.
        // therefore, each SV is an interval with a value interval.
        private readonly List<Interval<Interval>> _intervalFileLocations;
        private readonly long _baseFileLocation;
        public int Count=>_intervalFileLocations.Count;

        // data structures for query
        private readonly IntervalArray<Interval> _locationsTree;


        public IntervalChromIndex(long fileLocation)
        {
            _baseFileLocation = fileLocation;
            _intervalFileLocations = new List<Interval<Interval>>();
        }

        public IntervalChromIndex(ExtendedBinaryReader reader)
        {
            _baseFileLocation = reader.ReadOptInt64();
            int count = reader.ReadOptInt32();
            _intervalFileLocations= new List<Interval<Interval>>(count);

            for (var i = 0; i < count; i++)
            {
                int begin         = reader.ReadOptInt32();
                int end           = reader.ReadOptInt32();
                int startLocation = reader.ReadOptInt32();
                int endLocation   = reader.ReadOptInt32();
                _intervalFileLocations.Add(new Interval<Interval>(begin, end, new Interval(startLocation, endLocation)));
            }

            _locationsTree = new IntervalArray<Interval>(_intervalFileLocations.ToArray());
        }

        public (long startLocation, long endLocation) GetLocationRange(int start, int end)
        {
            if (_locationsTree == null) return (-1, -1);

            var overlappingEntries= _locationsTree.GetAllOverlappingValues(start, end);

            if (overlappingEntries == null) return (-1, -1);

            long startLocation = overlappingEntries[0].Start + _baseFileLocation;
            long endLocation = _baseFileLocation + overlappingEntries[overlappingEntries.Length - 1].End;

            return (startLocation, endLocation);
        }

        public void Add(int begin, int end, long startLocation, long endLocation)
        {
            var locationInterval = new Interval((int)(startLocation-_baseFileLocation), (int) (endLocation- _baseFileLocation));
            _intervalFileLocations.Add(new Interval<Interval>(begin, end, locationInterval));
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteOpt(_baseFileLocation);
            writer.WriteOpt(_intervalFileLocations.Count);
            foreach (var interval in _intervalFileLocations.OrderBy(x=>x.Begin).ThenBy(x=>x.End)) 
            {
                writer.WriteOpt(interval.Begin);
                writer.WriteOpt(interval.End);
                writer.WriteOpt(interval.Value.Start);
                writer.WriteOpt(interval.Value.End);
            }
        }
    }
}