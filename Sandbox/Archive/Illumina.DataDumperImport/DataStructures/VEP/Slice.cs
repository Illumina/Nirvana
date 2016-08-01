using System;

namespace Illumina.DataDumperImport.DataStructures.VEP
{
    public sealed class Slice : SortableCoordinate, IEquatable<Slice>
    {
        #region members

        private readonly bool _isCircular;
        private readonly bool _isTopLevel;
        private readonly bool _onReverseStrand;

        private readonly int _sequenceRegionLen;
        private readonly string _sequenceRegionName;

        private readonly int _hashCode;

        #endregion

        public Slice(ushort referenceIndex, int start, int end, bool onReverseStrand, bool isCircular, bool isTopLevel,   
            int sequenceRegionLen, string sequenceRegionName)
            : base(referenceIndex, start, end)
        {
            _onReverseStrand    = onReverseStrand;
            _isCircular         = isCircular;
            _isTopLevel         = isTopLevel;
            _sequenceRegionLen  = sequenceRegionLen;
            _sequenceRegionName = sequenceRegionName;

            _hashCode = End.GetHashCode()                ^
                        _isCircular.GetHashCode()         ^
                        _isTopLevel.GetHashCode()         ^
                        _onReverseStrand.GetHashCode()    ^
                        ReferenceIndex.GetHashCode()     ^
                        _sequenceRegionLen.GetHashCode()  ^
                        _sequenceRegionName.GetHashCode() ^
                        Start.GetHashCode();
        }

        #region Equality Overrides

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to Slice return false:
            var other = obj as Slice;
            if ((object)other == null) return false;

            // Return true if the fields match:
            return this == other;
        }

        bool IEquatable<Slice>.Equals(Slice other)
        {
            return Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        private bool Equals(Slice other)
        {
            return this == other;
        }

        public static bool operator ==(Slice a, Slice b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;

            return (a.End                == b.End)                &&
                   (a._isCircular         == b._isCircular)         &&
                   (a._isTopLevel         == b._isTopLevel)         &&
                   (a._onReverseStrand    == b._onReverseStrand)    &&
                   (a.ReferenceIndex     == b.ReferenceIndex)     &&
                   (a._sequenceRegionLen  == b._sequenceRegionLen)  &&
                   (a._sequenceRegionName == b._sequenceRegionName) &&
                   (a.Start              == b.Start);
        }

        public static bool operator !=(Slice a, Slice b)
        {
            return !(a == b);
        }

        #endregion

        /// <summary>
        /// returns a string representation of our exon
        /// </summary>
        public override string ToString()
        {
            return
                $"slice: {ReferenceIndex}: {Start} - {End}. {_sequenceRegionName} ({_sequenceRegionLen}). {(_onReverseStrand ? "R" : "F")}, {(_isCircular ? "C" : "L")}, {(_isTopLevel ? "top" : "not top")}";
        }
    }
}
