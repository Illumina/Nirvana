using System;
using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures
{
    public sealed class SimpleInterval : AnnotationInterval, IEquatable<SimpleInterval>, ICacheSerializable, IComparable<SimpleInterval>
    {
        /// <summary>
        /// constructor
        /// </summary>
        public SimpleInterval(int start, int end) : base(start, end) {}

        #region IEquatable Overrides

        public override int GetHashCode()
        {
            return Start.GetHashCode() ^ End.GetHashCode();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        public bool Equals(SimpleInterval value)
        {
            if (this == null) throw new NullReferenceException();
            if (value == null) return false;
            if (this == value) return true;
            return End == value.End && Start == value.Start;
        }

        #endregion

        /// <summary>
        /// reads the intron data from the binary reader
        /// </summary>
        public static SimpleInterval Read(ExtendedBinaryReader reader)
        {
            int start = reader.ReadOptInt32();
            int end   = reader.ReadOptInt32();
            return new SimpleInterval(start, end);
        }

	    public int CompareTo(SimpleInterval other)
	    {
		    if (Start != other.Start) return Start.CompareTo(other.Start);
		    if (End != other.End) return End.CompareTo(other.End);

		    return 0;

	    }

	    /// <summary>
        /// returns a string representation of our exon
        /// </summary>
        public override string ToString()
        {
            return $"{Start}\t{End}";
        }

        /// <summary>
        /// writes the intron data to the binary writer
        /// </summary>
        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteOpt(Start);
            writer.WriteOpt(End);
        }
    }
}
