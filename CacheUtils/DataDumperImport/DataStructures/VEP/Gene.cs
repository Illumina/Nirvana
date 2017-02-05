using System;

namespace CacheUtils.DataDumperImport.DataStructures.VEP
{
    public sealed class Gene : SortableCoordinate, IEquatable<Gene>
    {
        #region members

        public readonly string StableId;      // set
        private readonly bool _onReverseStrand; // set

        private readonly int _hashCode;

        #endregion

        public Gene(ushort referenceIndex, int start, int end, string stableId, bool onReverseStrand)
            : base(referenceIndex, start, end)
        {
            StableId        = stableId;
            _onReverseStrand = onReverseStrand;

            _hashCode = End.GetHashCode()             ^
                        _onReverseStrand.GetHashCode() ^
                        ReferenceIndex.GetHashCode()  ^
                        StableId.GetHashCode()        ^
                        Start.GetHashCode();
        }

        #region Equality Overrides

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to Gene return false:
            var other = obj as Gene;
            if ((object)other == null) return false;

            // Return true if the fields match:
            return this == other;
        }

        bool IEquatable<Gene>.Equals(Gene other)
        {
            return Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        private bool Equals(Gene gene)
        {
            return this == gene;
        }

        public static bool operator ==(Gene a, Gene b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if ((object)a == null || (object)b == null) return false;

            return a.End             == b.End             &&
                   a._onReverseStrand == b._onReverseStrand &&
                   a.ReferenceIndex  == b.ReferenceIndex  &&
                   a.StableId        == b.StableId        &&
                   a.Start           == b.Start;
        }

        public static bool operator !=(Gene a, Gene b)
        {
            return !(a == b);
        }

        #endregion

        /// <summary>
        /// returns a string representation of our gene
        /// </summary>
        public override string ToString()
        {
            return $"gene: {ReferenceIndex}: {Start} - {End}. {StableId} ({(_onReverseStrand ? "R" : "F")})";
        }
    }
}
