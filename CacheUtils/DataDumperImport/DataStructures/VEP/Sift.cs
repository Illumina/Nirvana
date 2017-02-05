using System;

namespace CacheUtils.DataDumperImport.DataStructures.VEP
{
    public sealed class Sift : IEquatable<Sift>
    {
        #region members

        public readonly string Matrix;
        private readonly int _hashCode;

        #endregion

        // constructor
        public Sift(string matrix)
        {
            Matrix = matrix;
            _hashCode = matrix.GetHashCode();
        }

        #region Equality Overrides

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to Sift return false:
            var other = obj as Sift;
            if (other == null) return false;

            // Return true if the fields match:
            return this == other;
        }

        bool IEquatable<Sift>.Equals(Sift other)
        {
            return Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        private bool Equals(Sift other)
        {
            return this == other;
        }

        public static bool operator ==(Sift a, Sift b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if ((object)a == null || (object)b == null) return false;

            return a.Matrix == b.Matrix;
        }

        public static bool operator !=(Sift a, Sift b)
        {
            return !(a == b);
        }

        #endregion
    }
}
