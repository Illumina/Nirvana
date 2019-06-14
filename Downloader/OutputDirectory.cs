using System.Collections.Generic;
using System.IO;
using Genome;

namespace Downloader
{
    public static class OutputDirectory
    {
        public static (string Cache, string Reference, string SupplementaryAnnotation) Create(string outputDirectory, IEnumerable<GenomeAssembly> genomeAssemblies)
        {
            string cacheDirectory      = Path.Combine(outputDirectory, "Cache");
            string referencesDirectory = Path.Combine(outputDirectory, "References");
            string saDirectory         = Path.Combine(outputDirectory, "SupplementaryAnnotation");

            // ReSharper disable PossibleMultipleEnumeration
            CreateGenomeAssemblySubdirectories(cacheDirectory, genomeAssemblies);
            CreateGenomeAssemblySubdirectories(saDirectory,    genomeAssemblies);
            // ReSharper restore PossibleMultipleEnumeration
            Directory.CreateDirectory(referencesDirectory);

            return (cacheDirectory, referencesDirectory, saDirectory);
        }

        private static void CreateGenomeAssemblySubdirectories(string topLevelDirectory, IEnumerable<GenomeAssembly> genomeAssemblies)
        {
            foreach (var genomeAssembly in genomeAssemblies)
            {
                string directory = Path.Combine(topLevelDirectory, genomeAssembly.ToString());
                Directory.CreateDirectory(directory);
            }
        }
    }
}
