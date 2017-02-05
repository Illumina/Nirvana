using System;

namespace VariantAnnotation.DataStructures
{
    public class AnnotationInterval
    {
        #region members

        public int Start;
        public int End;

        #endregion

        // constructor
        public AnnotationInterval(int start, int end)
        {
            Start = start;
            End   = end;
        }

        /// <summary>
        /// returns true if this object overlaps with the interval defined by the
        /// specified endpoints.
        /// </summary>
        public bool Overlaps(int begin, int end)
        {
            return end >= Start && begin <= End;
        }

        public bool CompletelyOverlaps(int begin, int end)
        {
            return begin <= Start && end >= End;
        }

        public double OverlapFraction(int begin, int end)
        {
            if (!Overlaps(begin, end)) return 0;// no overlap found

            var overlapStart = Math.Max(Start, begin);
            var overlapEnd = Math.Min(end, End);

            return (overlapEnd - overlapStart + 1) * 1.0 / (End - Start + 1);
        }
    }
}
