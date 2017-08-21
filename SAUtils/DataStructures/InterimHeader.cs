using System;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Providers;

namespace SAUtils.DataStructures
{
    public class InterimHeader
    {
        protected string Name { get; }
        public GenomeAssembly GenomeAssembly { get; }
        protected string Version { get; }
        protected string ReleaseDate { get; }
        private string Description { get; }


        protected InterimHeader(string name, string assembly, string version, string releaseDate, string description)
        {
            Name = name;
            GenomeAssembly = GenomeAssemblyUtilities.Convert(assembly);
            Version = version;
            Description = description;
            ReleaseDate = releaseDate;

        }

        public DataSourceVersion GetDataSourceVersion()
        {
            return new DataSourceVersion(
                Name, Version, DateTime.Parse(ReleaseDate).Ticks,
                Description);
        }

        public bool Equals(InterimHeader other)
        {
            if (other == null) return false;
            return Name.Equals(other.Name)
                   && GenomeAssembly == other.GenomeAssembly
                   && Version.Equals(other.Version)
                   && ReleaseDate.Equals(other.ReleaseDate);


        }

        public override string ToString()
        {
            return "dataSource=" + Name + ",version:" + Version + ",release date:" + ReleaseDate;
        }
    }
}