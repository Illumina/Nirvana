using System;

namespace Illumina.DataDumperImport.DataStructures.VEP
{
    public sealed class PolyPhen : IEquatable<PolyPhen>
    {
        #region members

        private readonly string _matrix;           // set
        private readonly int _hashCode;

        #endregion

        // constructor
        public PolyPhen(string matrix)
        {
            _matrix = matrix;
            _hashCode = matrix.GetHashCode();
        }

        #region Equality Overrides

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to PolyPhen return false:
            var other = obj as PolyPhen;
            if (other == null) return false;

            // Return true if the fields match:
            return this == other;
        }

        bool IEquatable<PolyPhen>.Equals(PolyPhen other)
        {
            return Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        private bool Equals(PolyPhen other)
        {
            return this == other;
        }

        public static bool operator ==(PolyPhen a, PolyPhen b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;

            return a._matrix == b._matrix;
        }

        public static bool operator !=(PolyPhen a, PolyPhen b)
        {
            return !(a == b);
        }

        #endregion

        /// <summary>
        /// converts the current VEP PolyPhen into a Nirvana PolyPhen 
        /// </summary>
        public VariantAnnotation.DataStructures.PolyPhen Convert()
        {
            // convert the base 64 string representation to our compressed prediction data
            var uncompressedDataWithHeader = System.Convert.FromBase64String(_matrix);
            const int headerLength = 3;

            // skip the 'VEP' header
            int newLength = uncompressedDataWithHeader.Length - headerLength;
            var uncompressedData = new byte[newLength];

            Buffer.BlockCopy(uncompressedDataWithHeader, headerLength, uncompressedData, 0, newLength);

            // write the uncompressed data to the PolyPhen object
            return new VariantAnnotation.DataStructures.PolyPhen(uncompressedData, false);
        }
    }
}
