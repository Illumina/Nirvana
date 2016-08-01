using System;
using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures
{
    public class MicroRna : AnnotationInterval, IEquatable<MicroRna>
    {
        #region members

        private readonly int _hashCode;

        #endregion

        // constructor
        private MicroRna(int start, int end)
            : base(start, end)
        {
            _hashCode = Start.GetHashCode() ^ End.GetHashCode();
        }

        #region Equality Overrides

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to MicroRna return false:
            var other = obj as MicroRna;
            if ((object)other == null) return false;

            // Return true if the fields match:
            return this == other;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        public bool Equals(MicroRna other)
        {
            return this == other;
        }

        public static bool operator ==(MicroRna a, MicroRna b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;

            return (a.End == b.End) && (a.Start == b.Start);
        }

        public static bool operator !=(MicroRna a, MicroRna b)
        {
            return !(a == b);
        }

        #endregion

        /// <summary>
        /// reads the miRNA data from the binary reader
        /// </summary>
        public static MicroRna Read(ExtendedBinaryReader reader)
        {
            int start = reader.ReadInt();
            int end   = reader.ReadInt();
            return new MicroRna(start, end);
        }

        /// <summary>
        /// writes the miRNA data to the binary writer
        /// </summary>
        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteInt(Start);
            writer.WriteInt(End);
        }
    }
}
