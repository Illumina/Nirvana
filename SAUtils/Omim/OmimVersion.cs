using System;
using System.IO;

namespace SAUtils.Omim
{
    public static class OmimVersion
    {
        private const string Name = "OMIM";
        private const string Description = "An Online Catalog of Human Genes and Genetic Disorders";
        private const string VersionFileSuffix = ".version";

        public static void WriteToFile(string outputPrefix, string outputDirectory)
        {
            using (var stream = new FileStream(Path.Combine(outputDirectory, outputPrefix + VersionFileSuffix), FileMode.Create))
            using (var writer = new StreamWriter(stream))
            {
                var currentDate = DateTime.Today;
                writer.WriteLine($"NAME={Name}");
                writer.WriteLine($"VERSION={currentDate:yyyyMMdd}");
                writer.WriteLine($"DATE={currentDate:yyyy-MM-dd}");
                writer.WriteLine($"DESCRIPTION={Description}");
            }
        }
    }
}