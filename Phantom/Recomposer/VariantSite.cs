using System;

namespace Phantom.Recomposer
{
    public struct VariantSite : IComparable<VariantSite>
    {
        public readonly int Start;
        public readonly string RefAllele;

        public VariantSite(int start, string refAllele)
        {
            Start = start;
            RefAllele = refAllele;
        }

        public int CompareTo(VariantSite other) => Start != other.Start ? Start.CompareTo(other.Start) : string.Compare(RefAllele, other.RefAllele, StringComparison.Ordinal);
    }
}