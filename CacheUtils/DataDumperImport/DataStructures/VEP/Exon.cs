using System;

namespace CacheUtils.DataDumperImport.DataStructures.VEP
{
    public class Exon : SortableCoordinate, IEquatable<Exon>
    {
        public readonly bool OnReverseStrand;

        public readonly byte? Phase;    // 0, 1, 2 (null if not set)

        private readonly string _stableId;

        private readonly int _hashCode;

        public Exon(ushort referenceIndex, int start, int end, string stableId, bool onReverseStrand, byte? phase)
            : base(referenceIndex, start, end)
        {
            _stableId        = stableId;
            OnReverseStrand = onReverseStrand;
            Phase           = phase;

            _hashCode = End.GetHashCode()             ^
                        OnReverseStrand.GetHashCode() ^
                        ReferenceIndex.GetHashCode()  ^
                        _stableId.GetHashCode()       ^
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
            if ((object)a == null || (object)b == null) return false;

            return a.End             == b.End             &&
                   a.OnReverseStrand == b.OnReverseStrand &&
                   a.ReferenceIndex  == b.ReferenceIndex  &&
                   a._stableId       == b._stableId       &&
                   a.Start           == b.Start;
        }

        public static bool operator !=(Exon a, Exon b)
        {
            return !(a == b);
        }

        #endregion

        /// <summary>
        /// converts the current VEP exon into a Nirvana exon 
        /// </summary>
        public LegacyExon Convert()
        {
            return new LegacyExon(Start, End, Phase);
        }

        /// <summary>
        /// returns a string representation of our exon
        /// </summary>
        public override string ToString()
        {
            return
                $"exon: {ReferenceIndex}: {Start} - {End}. {_stableId} ({(OnReverseStrand ? "R" : "F")}), phase: {Phase}";
        }
    }
}
