using System;
using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures
{
    public class CdnaCoordinateMap : IEquatable<CdnaCoordinateMap>
    {
        #region members

        public readonly AnnotationInterval Genomic;
        public readonly AnnotationInterval CodingDna;
        private readonly int _hashCode;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        private CdnaCoordinateMap(AnnotationInterval genomic, AnnotationInterval codingDna)
        {
            Genomic   = genomic;
            CodingDna = codingDna;

            _hashCode = Genomic.Start.GetHashCode()   ^
                        Genomic.End.GetHashCode()     ^
                        CodingDna.Start.GetHashCode() ^
                        CodingDna.End.GetHashCode();
        }

        #region Equality Overrides

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to CdnaCoordinateMap return false:
            var other = obj as CdnaCoordinateMap;
            if ((object)other == null) return false;

            // Return true if the fields match:
            return this == other;
        }

        bool IEquatable<CdnaCoordinateMap>.Equals(CdnaCoordinateMap other)
        {
            return Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        private bool Equals(CdnaCoordinateMap cdnaMap)
        {
            return this == cdnaMap;
        }

        public static bool operator ==(CdnaCoordinateMap a, CdnaCoordinateMap b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;

            return (a.Genomic.Start   == b.Genomic.Start) &&
                   (a.Genomic.End     == b.Genomic.End) &&
                   (a.CodingDna.Start == b.CodingDna.Start) &&
                   (a.CodingDna.End   == b.CodingDna.End);
        }

        public static bool operator !=(CdnaCoordinateMap a, CdnaCoordinateMap b)
        {
            return !(a == b);
        }

        #endregion

        /// <summary>
        /// reads the cDNA coordinate map from the binary reader
        /// </summary>
        public static CdnaCoordinateMap Read(ExtendedBinaryReader reader)
        {
            // read the genomic interval
            int genomicStart = reader.ReadInt();
            int genomicEnd   = reader.ReadInt();

            // read the cDNA interval
            int cdnaStart = reader.ReadInt();
            int cdnaEnd   = reader.ReadInt();

            return new CdnaCoordinateMap(new AnnotationInterval(genomicStart, genomicEnd), 
                new AnnotationInterval(cdnaStart, cdnaEnd));
        }

        /// <summary>
        /// writes the cDNA coordinate map to the binary writer
        /// </summary>
        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteInt(Genomic.Start);
            writer.WriteInt(Genomic.End);
            writer.WriteInt(CodingDna.Start);
            writer.WriteInt(CodingDna.End);
        }
    }
}
