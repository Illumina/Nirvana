using System;
using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures
{
    public class Intron : AnnotationInterval, IEquatable<Intron>
    {
        #region members

        private readonly int _hashCode;

        #endregion

        // constructor
        internal Intron(int start, int end) : base(start, end)
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
            // If parameter cannot be cast to Intron return false:
            var other = obj as Intron;
            if ((object)other == null) return false;

            // Return true if the fields match:
            return this == other;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        public bool Equals(Intron intron)
        {
            return this == intron;
        }

        public static bool operator ==(Intron a, Intron b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;

            return (a.End == b.End) && (a.Start == b.Start);
        }

        public static bool operator !=(Intron a, Intron b)
        {
            return !(a == b);
        }

        #endregion

        /// <summary>
        /// reads the intron data from the binary reader
        /// </summary>
        public static Intron Read(ExtendedBinaryReader reader)
        {
            int start = reader.ReadInt();
            int end   = reader.ReadInt();
            return new Intron(start, end);
        }

        /// <summary>
        /// writes the intron data to the binary writer
        /// </summary>
        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteInt(Start);
            writer.WriteInt(End);
        }
    }
}
