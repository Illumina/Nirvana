using System;
using VariantAnnotation.Providers;

namespace SAUtils.Omim
{
    public static class OmimVersion
    {
        private const string Name = "OMIM";
        private const string Description = "An Online Catalog of Human Genes and Genetic Disorders";

        public static DataSourceVersion GetVersion()
        {
            var currentDate = DateTime.Today;
            var version = currentDate.ToString("yyyyMMdd");
            var ticks = currentDate.Ticks;

            return new DataSourceVersion(Name, version, ticks, Description);
        }
    }
}