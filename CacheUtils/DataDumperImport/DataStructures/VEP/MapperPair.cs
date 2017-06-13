using System;

namespace CacheUtils.DataDumperImport.DataStructures.VEP
{
    public sealed class MapperPair : IEquatable<MapperPair>
    {
        #region members

        public readonly MapperUnit From; // null
        public readonly MapperUnit To; // null

        private readonly int _hashCode;

        #endregion

        // constructor
        public MapperPair(MapperUnit from, MapperUnit to)
        {
            From = from;
            To = to;

            _hashCode = From.GetHashCode() ^ To.GetHashCode();
        }

        #region Equality Overrides

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to a mapper pair return false:
            var other = obj as MapperPair;
            if (other == null) return false;

            // Return true if the fields match:
            return this == other;
        }

        bool IEquatable<MapperPair>.Equals(MapperPair other)
        {
            return Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        private bool Equals(MapperPair other)
        {
            return this == other;
        }

        public static bool operator ==(MapperPair a, MapperPair b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if ((object)a == null || (object)b == null) return false;

            return a.From == b.From &&
                   a.To == b.To;
        }

        public static bool operator !=(MapperPair a, MapperPair b)
        {
            return !(a == b);
        }

        #endregion
    }
}

