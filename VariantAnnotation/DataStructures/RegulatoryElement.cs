using System;
using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures
{
    public sealed class RegulatoryElement : ReferenceAnnotationInterval, IEquatable<RegulatoryElement>, ICacheSerializable
    {
        #region members

        public readonly CompactId Id;
        public readonly RegulatoryElementType Type;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public RegulatoryElement(ushort referenceIndex, int start, int end, CompactId id, RegulatoryElementType type)
            : base(referenceIndex, start, end)
        {
            Id   = id;
            Type = type;
        }

        #region IEquatable Overrides

        public override int GetHashCode()
        {
            return Start.GetHashCode() ^ End.GetHashCode() ^ Id.GetHashCode();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        public bool Equals(RegulatoryElement value)
        {
            if (this == null) throw new NullReferenceException();
            if (value == null) return false;
            if (this == value) return true;
            return Start == value.Start && End == value.End && Id.Equals(value.Id);
        }

        #endregion

        /// <summary>
        /// reads the regulatory element data from the binary reader
        /// </summary>
        public static RegulatoryElement Read(ExtendedBinaryReader reader)
        {
            var referenceIndex = reader.ReadUInt16();
            int start          = reader.ReadOptInt32();
            int end            = reader.ReadOptInt32();
            var type           = (RegulatoryElementType)reader.ReadByte();
            var id             = CompactId.Read(reader);

            return new RegulatoryElement(referenceIndex, start, end, id, type);
        }

        /// <summary>
        /// returns a string representation of our regulatory element
        /// </summary>
        public override string ToString()
        {
            return $"regulatory element: {Start} - {End}. ID: {Id}";
        }

        /// <summary>
        /// writes the regulatory element data to the binary writer
        /// </summary>
        public void Write(ExtendedBinaryWriter writer)
        {
            writer.Write(ReferenceIndex);
            writer.WriteOpt(Start);
            writer.WriteOpt(End);
            writer.Write((byte)Type);
            Id.Write(writer);
        }
    }
}
