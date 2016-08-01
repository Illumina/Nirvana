using System;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
    public abstract class SupplementaryDataItem : IComparable<SupplementaryDataItem>, IEquatable<SupplementaryDataItem>
    {
        public string Chromosome { get; protected set; }
        public int Start { get; protected set; }
        public bool IsInterval { get; protected set; }
        public abstract SupplementaryDataItem SetSupplementaryAnnotations(SupplementaryAnnotation sa, string refBases = null);
        public abstract SupplementaryInterval GetSupplementaryInterval();

        public int CompareTo(SupplementaryDataItem otherItem)
        {
            if (otherItem == null) return -1;
            if (Chromosome.Equals(otherItem.Chromosome)) return Start.CompareTo(otherItem.Start);
            return string.CompareOrdinal(Chromosome, otherItem.Chromosome);
        }

        public bool Equals(SupplementaryDataItem other)
        {
            if (other == null) return false;
            return Start.Equals(other.Start) && Chromosome.Equals(other.Chromosome);
        }
    }
}
