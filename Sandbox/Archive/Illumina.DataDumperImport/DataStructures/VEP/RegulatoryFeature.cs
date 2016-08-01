using System;

namespace Illumina.DataDumperImport.DataStructures.VEP
{
    public sealed class RegulatoryFeature : SortableCoordinate, IEquatable<RegulatoryFeature>
    {
        #region members

        public readonly string StableId;

        private readonly int _hashCode;

        #endregion

        // constructor
        public RegulatoryFeature(ushort referenceIndex, int start, int end, string stableId)
            : base(referenceIndex, start, end)
        {
            StableId = stableId;

            _hashCode = End.GetHashCode() ^
                        StableId.GetHashCode() ^
                        Start.GetHashCode();
        }

        #region Equality Overrides

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to regulatory feature return false:
            var other = obj as RegulatoryFeature;
            if ((object)other == null) return false;

            // Return true if the fields match:
            return this == other;
        }

        bool IEquatable<RegulatoryFeature>.Equals(RegulatoryFeature other)
        {
            return Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        private bool Equals(RegulatoryFeature other)
        {
            return this == other;
        }

        public static bool operator ==(RegulatoryFeature a, RegulatoryFeature b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;

            return (a.End      == b.End)      &&
                   (a.StableId == b.StableId) &&
                   (a.Start    == b.Start);
        }

        public static bool operator !=(RegulatoryFeature a, RegulatoryFeature b)
        {
            return !(a == b);
        }

        #endregion

        /// <summary>
        /// returns a string representation of our regulatory feature
        /// </summary>
        public override string ToString()
        {
            return $"regulatory feature: {ReferenceIndex}: {Start} - {End}. {StableId}";
        }
    }
}
