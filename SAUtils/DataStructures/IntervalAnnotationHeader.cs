using System;
using VariantAnnotation.Interface.SA;

namespace SAUtils.DataStructures
{
    public sealed class IntervalAnnotationHeader : SaHeader, IEquatable<IntervalAnnotationHeader>
    {
        private readonly ReportFor _reportingFor;

        public IntervalAnnotationHeader(string name, string assembly, string version, string releaseDate, string description,
            ReportFor reportingFor) : base(name, assembly, version, releaseDate, description)
        {
            _reportingFor = reportingFor;
        }

        public bool Equals(IntervalAnnotationHeader other)
        {
            if (other == null) return false;
            return Name.Equals(other.Name)
                   && GenomeAssembly == other.GenomeAssembly
                   && Version.Equals(other.Version)
                   && ReleaseDate.Equals(other.ReleaseDate);
        }

        public override string ToString()
        {
            return $"{Name,-20}   {Version,-14}   {ReleaseDate,-10}   {_reportingFor,-18}";
        }
    }
}