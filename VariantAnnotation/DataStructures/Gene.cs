using System;
using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures
{
    public class Gene : AnnotationInterval, IEquatable<Gene>
    {
        #region members

        public string Symbol { get; }
        private readonly int _hashCode;

        #endregion

        // constructor
        private Gene(string symbol, int start, int end) : base(start, end)
        {
            Symbol    = symbol;
            _hashCode = symbol.GetHashCode() ^ End.GetHashCode() ^ Start.GetHashCode();
        }

        #region IEquatable methods

        public override bool Equals(object obj)
        {
            var other = obj as Gene;
            if (other == null) return false;

            return this == other;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        bool IEquatable<Gene>.Equals(Gene other)
        {
            return this == other;
        }
        public static bool operator ==(Gene a, Gene b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;

            return (a.End == b.End) && (a.Start == b.Start);
        }

        public static bool operator !=(Gene a, Gene b)
        {
            return !(a == b);
        }

        #endregion

        /// <summary>
        /// reads the gene data from the binary reader
        /// </summary>
        public static Gene Read(ExtendedBinaryReader reader)
        {
            string symbol = reader.ReadAsciiString();
            int start     = reader.ReadInt();
            int end       = reader.ReadInt();

            return new Gene(symbol, start, end);
        }

        /// <summary>
        /// returns a string representation of our gene
        /// </summary>
        public override string ToString()
        {
            return $"gene: {Start} - {End} ({Symbol})";
        }

        /// <summary>
        /// writes the gene data to the binary writer
        /// </summary>
        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteAsciiString(Symbol);
            writer.WriteInt(Start);
            writer.WriteInt(End);
        }
    }
}
