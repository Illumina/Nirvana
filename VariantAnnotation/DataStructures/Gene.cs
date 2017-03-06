using System;
using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures
{
    public sealed class Gene : ReferenceAnnotationInterval, IEquatable<Gene>, ICacheSerializable
    {
        public readonly bool OnReverseStrand;
        public readonly string Symbol;
        public readonly CompactId EntrezGeneId;
        public readonly CompactId EnsemblId;
        private readonly int _hgncId;
        private readonly int _mimNumber;

        /// <summary>
        /// constructor
        /// </summary>
        public Gene(ushort referenceIndex, int start, int end, bool onReverseStrand, string symbol, int hgncId,
            CompactId entrezGeneId, CompactId ensemblId, int mimNumber) : base(referenceIndex, start, end)
        {
            OnReverseStrand = onReverseStrand;
            Symbol          = symbol;
            _hgncId         = hgncId;
            EntrezGeneId    = entrezGeneId;
            EnsemblId       = ensemblId;
            _mimNumber      = mimNumber;
        }

        #region IEquatable methods

        public override int GetHashCode()
        {
            var hashCode = ReferenceIndex.GetHashCode() ^ Start.GetHashCode() ^ End.GetHashCode() ^
                           EntrezGeneId.GetHashCode() ^ EnsemblId.GetHashCode() ^ _hgncId.GetHashCode();
            if (Symbol != null) hashCode ^= Symbol.GetHashCode();
            return hashCode;
        }

        public bool Equals(Gene value)
        {
            if (this == null) throw new NullReferenceException();
            if (value == null) return false;
            if (this == value) return true;
            return ReferenceIndex == value.ReferenceIndex && End == value.End && Start == value.Start &&
                   _hgncId == value._hgncId && Symbol == value.Symbol && EntrezGeneId.Equals(value.EntrezGeneId) &&
                   EnsemblId.Equals(value.EnsemblId);
        }

        #endregion

        /// <summary>
        /// reads the gene data from the binary reader
        /// </summary>
        public static Gene Read(ExtendedBinaryReader reader)
        {
            ushort referenceIndex = reader.ReadUInt16();
            int start             = reader.ReadOptInt32();
            int end               = reader.ReadOptInt32();
            bool onReverseStrand  = reader.ReadBoolean();
            string symbol         = reader.ReadAsciiString();
            int hgncId            = reader.ReadOptInt32();
            var entrezId          = CompactId.Read(reader);
            var ensemblId         = CompactId.Read(reader);
            int mimNumber         = reader.ReadOptInt32();

            return new Gene(referenceIndex, start, end, onReverseStrand, symbol, hgncId, entrezId, ensemblId, mimNumber);
        }

        /// <summary>
        /// returns a string representation of our gene
        /// </summary>
        public override string ToString()
        {
            var strand = OnReverseStrand ? 'R' : 'F';
            var hgncId = _hgncId == -1 ? "" : _hgncId.ToString();
            return $"{ReferenceIndex}\t{Start}\t{End}\t{strand}\t{Symbol}\t{hgncId}\t{EntrezGeneId}\t{EnsemblId}\t{_mimNumber}";
        }

        /// <summary>
        /// writes the gene data to the binary writer
        /// </summary>
        public void Write(ExtendedBinaryWriter writer)
        {
            writer.Write(ReferenceIndex);
            writer.WriteOpt(Start);
            writer.WriteOpt(End);
            writer.Write(OnReverseStrand);
            writer.WriteOptAscii(Symbol);
            writer.WriteOpt(_hgncId);
            // ReSharper disable ImpureMethodCallOnReadonlyValueField
            EntrezGeneId.Write(writer);
            EnsemblId.Write(writer);
            // ReSharper restore ImpureMethodCallOnReadonlyValueField
            writer.WriteOpt(_mimNumber);
        }
    }
}
