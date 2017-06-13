using System;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.Binary;

namespace VariantAnnotation.DataStructures.Transcript
{
    public struct CdnaCoordinateMap : IEquatable<CdnaCoordinateMap>, ICacheSerializable
    {
        #region members

        public readonly int GenomicStart;
        public readonly int GenomicEnd;
        public readonly int CdnaStart;
        public readonly int CdnaEnd;

        public bool IsNull => GenomicStart == -1 && GenomicEnd == -1 && CdnaStart == -1 && CdnaEnd == -1;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public CdnaCoordinateMap(int genomicStart, int genomicEnd, int cdnaStart, int cdnaEnd)
        {
            GenomicStart = genomicStart;
            GenomicEnd   = genomicEnd;
            CdnaStart    = cdnaStart;
            CdnaEnd      = cdnaEnd;
        }

        #region IEquatable Overrides

        public override int GetHashCode()
        {
            return GenomicStart.GetHashCode() ^ GenomicEnd.GetHashCode() ^ CdnaStart.GetHashCode() ^
                   CdnaEnd.GetHashCode();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        public bool Equals(CdnaCoordinateMap value)
        {
            return GenomicStart == value.GenomicStart && GenomicEnd == value.GenomicEnd &&
                   CdnaStart == value.CdnaStart && CdnaEnd == value.CdnaEnd;
        }

        #endregion

        public static CdnaCoordinateMap Null() => new CdnaCoordinateMap(-1, -1, -1, -1);

        /// <summary>
        /// reads the cDNA coordinate map from the binary reader
        /// </summary>
        public static CdnaCoordinateMap Read(ExtendedBinaryReader reader)
        {
            // read the genomic interval
            int genomicStart = reader.ReadOptInt32();
            int genomicEnd   = reader.ReadOptInt32();

            // read the cDNA interval
            int cdnaStart = reader.ReadOptInt32();
            int cdnaEnd   = reader.ReadOptInt32();

            return new CdnaCoordinateMap(genomicStart, genomicEnd, cdnaStart, cdnaEnd);
        }

        public override string ToString()
        {
            return $"{GenomicStart}\t{GenomicEnd}\t{CdnaStart}\t{CdnaEnd}";
        }

        /// <summary>
        /// writes the cDNA coordinate map to the binary writer
        /// </summary>
        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteOpt(GenomicStart);
            writer.WriteOpt(GenomicEnd);
            writer.WriteOpt(CdnaStart);
            writer.WriteOpt(CdnaEnd);
        }
    }
}
