using System;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
    public class InterimSaHeader : InterimHeader, IEquatable<InterimSaHeader>
    {
        private bool IsAlleleSpecific { get; }

        public InterimSaHeader(string name, string assembly, string version, string releaseDate, string description,
            bool isAlleleSpecific) : base(name, assembly, version, releaseDate, description)
        {
            IsAlleleSpecific = isAlleleSpecific;
        }

        public bool Equals(InterimSaHeader other)
        {
            if (other == null) return false;
            return Name.Equals(other.Name)
                   && GenomeAssembly == other.GenomeAssembly
                   && Version.Equals(other.Version)
                   && ReleaseDate.Equals(other.ReleaseDate)
                   && IsAlleleSpecific == other.IsAlleleSpecific;
        }

        public override string ToString()
        {
            return $"{Name,-20}   {Version,-14}   {ReleaseDate,-10}   {IsAlleleSpecific,-18}";
        }
    }
}
