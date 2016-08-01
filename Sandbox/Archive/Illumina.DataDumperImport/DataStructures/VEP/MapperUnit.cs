using System;

namespace Illumina.DataDumperImport.DataStructures.VEP
{
    public sealed class MapperUnit : SortableCoordinate, IEquatable<MapperUnit>
    {
        #region members

        public readonly MapperUnitType Id;
        private readonly int _hashCode;

        #endregion

        // constructor
        public MapperUnit(ushort referenceIndex, int start, int end, MapperUnitType id)
            : base(referenceIndex, start, end)
        {
            Id = id;

            _hashCode = End.GetHashCode()            ^
                        Id.GetHashCode()             ^
                        ReferenceIndex.GetHashCode() ^
                        Start.GetHashCode();
        }

        #region Equality Overrides

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to a mapper unit return false:
            var other = obj as MapperUnit;
            if (other == null) return false;

            // Return true if the fields match:
            return this == other;
        }

        bool IEquatable<MapperUnit>.Equals(MapperUnit other)
        {
            return Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        private bool Equals(MapperUnit other)
        {
            return this == other;
        }

        public static bool operator ==(MapperUnit a, MapperUnit b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;

            return (a.End            == b.End)            &&
                   (a.Id             == b.Id)             &&
                   (a.ReferenceIndex == b.ReferenceIndex) &&
                   (a.Start          == b.Start);
        }

        public static bool operator !=(MapperUnit a, MapperUnit b)
        {
            return !(a == b);
        }

        #endregion

        /// <summary>
        /// returns a string representation of our exon
        /// </summary>
        public override string ToString()
        {
            return $"mapper unit: {ReferenceIndex}: {Start} - {End}. ID: {Id}";
        }
    }

    public enum MapperUnitType : byte
    {
        CodingDna,
        Genomic,
        Unknown
    }
}
