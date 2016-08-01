using System;
using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures
{
    public class Exon : AnnotationInterval, IEquatable<Exon>
    {
        #region members

        public readonly int Phase; // 0, 1, 2

        private readonly int _hashCode;

        #endregion

        // constructor
        private Exon(int start, int end, int phase)
            : base(start, end)
        {
            Phase    = phase;

            _hashCode = End.GetHashCode()   ^
                        Phase.GetHashCode() ^
                        Start.GetHashCode();
        }

        #region Equality Overrides

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to Exon return false:
            var other = obj as Exon;
            if ((object)other == null) return false;

            // Return true if the fields match:
            return this == other;
        }

        bool IEquatable<Exon>.Equals(Exon other)
        {
            return Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        private bool Equals(Exon exon)
        {
            return this == exon;
        }

        public static bool operator ==(Exon a, Exon b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;

            return (a.End   == b.End)   &&
                   (a.Phase == b.Phase) &&
                   (a.Start == b.Start);
        }

        public static bool operator !=(Exon a, Exon b)
        {
            return !(a == b);
        }

        #endregion

        /// <summary>
        /// reads the exon data from the binary reader
        /// </summary>
        public static Exon Read(ExtendedBinaryReader reader)
        {
            int phase = reader.ReadByte() - 1;
            int start = reader.ReadInt();
            int end   = reader.ReadInt();

            return new Exon(start, end, phase);
        }

        /// <summary>
        /// returns a string representation of our exon
        /// </summary>
        public override string ToString()
        {
            return $"exon: {Start} - {End}. phase: {Phase}";
        }

        /// <summary>
        /// writes the exon data to the binary writer
        /// </summary>
        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteByte((byte)(Phase + 1));
            writer.WriteInt(Start);
            writer.WriteInt(End);
        }
    }
}
