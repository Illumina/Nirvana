using System;
using System.Linq;
using Illumina.VariantAnnotation.DataStructures;

namespace Illumina.DataDumperImport.DataStructures.VEP
{
    public sealed class Transcript : SortableCoordinate, IEquatable<Transcript>
    {
        #region members

        // use null values to detect downstream annotation problems
        public readonly BioType BioType;
        public readonly MicroRna[] MicroRnas;
        public readonly Exon[] TransExons;
        public Gene Gene;               // null
        public readonly Translation Translation; // null
        public readonly VariantEffectFeatureCache VariantEffectCache;
        public Slice Slice;             // null

        public readonly bool OnReverseStrand;   // set
        public bool IsCanonical;       // set

        public readonly int CompDnaCodingStart; // set
        public readonly int CompDnaCodingEnd;   // set

        public readonly byte Version;

        public readonly string CcdsId;          // set // null
        private readonly string _databaseId;      // set // null
        public readonly string ProteinId;       // set // null
        private readonly string _refSeqId;        // set // null
        public readonly string GeneStableId;    // set // null
        public readonly string StableId;        // set // null

        public readonly string GeneSymbol;
        public readonly GeneSymbolSource GeneSymbolSource;
        public readonly string HgncId;

        private int _hashCode;

        #endregion

        public Transcript(BioType biotype, Exon[] transExons, Gene gene, Translation translation, VariantEffectFeatureCache cache, Slice slice,
            bool onReverseStrand, bool isCanonical, int cdnaCodingStart, int cdnaCodingEnd, ushort referenceIndex, int start, int end, 
            string ccdsId, string databaseId, string proteinId, string refSeqId, string geneStableId, string stableId, string geneSymbol, 
            GeneSymbolSource geneSymbolSource, string hgncId, byte version, MicroRna[] microRnas)
            : base(referenceIndex, start, end)
        {
            BioType            = biotype;
            CcdsId             = ccdsId;
            CompDnaCodingEnd   = cdnaCodingEnd;
            CompDnaCodingStart = cdnaCodingStart;
            _databaseId         = databaseId;
            Gene               = gene;
            GeneStableId       = geneStableId;
            GeneSymbol         = geneSymbol;
            GeneSymbolSource   = geneSymbolSource;
            HgncId             = hgncId;
            IsCanonical        = isCanonical;
            MicroRnas          = microRnas;
            OnReverseStrand    = onReverseStrand;
            ProteinId          = proteinId;
            _refSeqId           = refSeqId;
            Slice              = slice;
            StableId           = stableId;
            TransExons         = transExons;
            Translation        = translation;
            VariantEffectCache = cache;
            Version            = version;

            GenerateHashCode();
        }

        /// <summary>
        /// generates the hash code
        /// </summary>
        private void GenerateHashCode()
        {
            _hashCode = BioType.GetHashCode()            ^
                        CompDnaCodingEnd.GetHashCode()   ^
                        CompDnaCodingStart.GetHashCode() ^
                        End.GetHashCode()                ^
                        IsCanonical.GetHashCode()        ^
                        OnReverseStrand.GetHashCode()    ^
                        ReferenceIndex.GetHashCode()     ^
                        Start.GetHashCode();

            if (CcdsId       != null) _hashCode ^= CcdsId.GetHashCode();
            if (_databaseId   != null) _hashCode ^= _databaseId.GetHashCode();
            if (ProteinId    != null) _hashCode ^= ProteinId.GetHashCode();
            if (_refSeqId     != null) _hashCode ^= _refSeqId.GetHashCode();
            if (GeneStableId != null) _hashCode ^= GeneStableId.GetHashCode();
            if (StableId     != null) _hashCode ^= StableId.GetHashCode();

            if (Gene  != null) _hashCode ^= Gene.GetHashCode();
            if (Slice != null) _hashCode ^= Slice.GetHashCode();
        }

        #region Equality Overrides

        // ReSharper disable once NonReadonlyFieldInGetHashCode
        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to Transcript return false:
            var other = obj as Transcript;
            if ((object)other == null) return false;

            // Return true if the fields match:
            return this == other;
        }

        bool IEquatable<Transcript>.Equals(Transcript other)
        {
            return Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        private bool Equals(Transcript transcript)
        {
            return this == transcript;
        }

        public static bool operator ==(Transcript a, Transcript b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;

            return (a.BioType            == b.BioType)            &&
                   (a.CcdsId             == b.CcdsId)             &&
                   (a.CompDnaCodingEnd   == b.CompDnaCodingEnd)   &&
                   (a.CompDnaCodingStart == b.CompDnaCodingStart) &&
                   (a._databaseId         == b._databaseId)         &&
                   (a.End                == b.End)                &&
                   (a.Gene               == b.Gene)               &&
                   (a.GeneStableId       == b.GeneStableId)       &&
                   (a.IsCanonical        == b.IsCanonical)        &&
                   (a.OnReverseStrand    == b.OnReverseStrand)    &&
                   (a.ProteinId          == b.ProteinId)          &&
                   (a.ReferenceIndex     == b.ReferenceIndex)     &&
                   (a._refSeqId           == b._refSeqId)           &&
                   (a.Slice              == b.Slice)              &&
                   (a.StableId           == b.StableId)           &&
                   (a.Start              == b.Start);
        }

        public static bool operator !=(Transcript a, Transcript b)
        {
            return !(a == b);
        }

        #endregion

        /// <summary>
        /// returns the start position of the coding region. Returns -1 if no translation was possible.
        /// </summary>
        public int GetCodingRegionStart()
        {
            // sanity check: make sure that translation is not null
            if (Translation == null) return -1;

            return Translation.StartExon.OnReverseStrand
                ? Translation.EndExon.End     - Translation.End   + 1
                : Translation.StartExon.Start + Translation.Start - 1;
        }

        /// <summary>
        /// returns the start position of the coding region. Returns -1 if no translation was possible.
        /// </summary>
        public int GetCodingRegionEnd()
        {
            // sanity check: make sure that translation is not null
            if (Translation == null) return -1;

            return Translation.StartExon.OnReverseStrand
                ? Translation.StartExon.End - Translation.Start + 1
                : Translation.EndExon.Start + Translation.End - 1;
        }

        /// <summary>
        /// returns the sum of the exon lengths
        /// </summary>
        public int GetTotalExonLength()
        {
            return TransExons.Sum(exon => exon.End - exon.Start + 1);
        }


        /// <summary>
        /// returns a string representation of our exon
        /// </summary>
        public override string ToString()
        {
            return $"transcript: {ReferenceIndex}: {Start} - {End}. {StableId} ({(OnReverseStrand ? "R" : "F")})";
        }
    }
}
