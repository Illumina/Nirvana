using System;
using Genome;
using VariantAnnotation.Providers;

namespace SAUtils.DataStructures
{
    public class SaHeader
    {
        protected string Name { get; }
        public GenomeAssembly Assembly { get; }
        protected string Version { get; }
        protected string ReleaseDate { get; }
        private string Description { get; }


        public SaHeader(string name, string assembly, string version, string releaseDate, string description)
        {
            Name        = name;
            Assembly    = GenomeAssemblyHelper.Convert(assembly);
            Version     = version;
            Description = description;
            ReleaseDate = releaseDate;
        }

        public DataSourceVersion GetDataSourceVersion()
        {
            return new DataSourceVersion(
                Name, Version, DateTime.Parse(ReleaseDate).Ticks,
                Description);
        }

        public override string ToString()
        {
            return $"{Name,-20}   {Version,-14}   {ReleaseDate,-10}";
        }
    }
}