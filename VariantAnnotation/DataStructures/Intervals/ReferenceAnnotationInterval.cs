using System;

namespace VariantAnnotation.DataStructures.Intervals
{
    public class ReferenceAnnotationInterval:IComparable<ReferenceAnnotationInterval>
    {
        public readonly ushort ReferenceIndex;
        public readonly int Start;
        public readonly int End;

        /// <summary>
        /// constructor
        /// </summary>
        public ReferenceAnnotationInterval(ushort referenceIndex, int start, int end)
        {
            ReferenceIndex = referenceIndex;
            Start          = start;
            End            = end;
        }

	    public int CompareTo(ReferenceAnnotationInterval other)
	    {
		    if (ReferenceIndex != other.ReferenceIndex) return ReferenceIndex.CompareTo(other.ReferenceIndex);
		    if (Start != other.Start) return Start.CompareTo(other.Start);
			if (End != other.End) return End.CompareTo(other.End);

		    return 0;
	    }
    }
}
