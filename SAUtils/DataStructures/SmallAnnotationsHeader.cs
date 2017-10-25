using System;

namespace SAUtils.DataStructures
{
    public sealed class SmallAnnotationsHeader : SaHeader, IEquatable<SmallAnnotationsHeader>
    {
        private bool MatchByAllele { get; }

        public SmallAnnotationsHeader(string name, string assembly, string version, string releaseDate, string description,
            bool matchByAllele) : base(name, assembly, version, releaseDate, description)
        {
            MatchByAllele = matchByAllele;
        }

        public bool Equals(SmallAnnotationsHeader other)
        {
            if (other == null) return false;
            return Name.Equals(other.Name)
                   && GenomeAssembly == other.GenomeAssembly
                   && Version.Equals(other.Version)
                   && ReleaseDate.Equals(other.ReleaseDate)
                   && MatchByAllele == other.MatchByAllele;
        }

        public override string ToString()
        {
            return $"{Name,-20}   {Version,-14}   {ReleaseDate,-10}   {MatchByAllele,-18}";
        }
    }
}