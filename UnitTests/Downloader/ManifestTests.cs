using System.Collections.Generic;
using System.Linq;
using Downloader;
using Genome;
using Xunit;

namespace UnitTests.Downloader
{
    public sealed class ManifestTests
    {
        private const string ManifestGRCh37 = "Manifest_GRCh37";
        private const string ManifestGRCh38 = "Manifest_GRCh38";

        [Fact]
        public void CreateGenomeAssemblyPaths_GRCh37()
        {
            var genomeAssemblies = new List<GenomeAssembly> { GenomeAssembly.GRCh37 };
            List<(GenomeAssembly GenomeAssembly, string ManifestPath)> list = Manifest.CreateGenomeAssemblyPaths(ManifestGRCh37, ManifestGRCh38, genomeAssemblies).ToList();
            Assert.Single(list);
            Assert.Equal(GenomeAssembly.GRCh37, list[0].GenomeAssembly);
            Assert.Equal(ManifestGRCh37, list[0].ManifestPath);
        }

        [Fact]
        public void CreateGenomeAssemblyPaths_GRCh38()
        {
            var genomeAssemblies = new List<GenomeAssembly> { GenomeAssembly.GRCh38 };
            List<(GenomeAssembly GenomeAssembly, string ManifestPath)> list = Manifest.CreateGenomeAssemblyPaths(ManifestGRCh37, ManifestGRCh38, genomeAssemblies).ToList();
            Assert.Single(list);
            Assert.Equal(GenomeAssembly.GRCh38, list[0].GenomeAssembly);
            Assert.Equal(ManifestGRCh38, list[0].ManifestPath);
        }

        [Fact]
        public void CreateGenomeAssemblyPaths_Both()
        {
            var genomeAssemblies = new List<GenomeAssembly> { GenomeAssembly.GRCh37, GenomeAssembly.GRCh38 };
            List<(GenomeAssembly GenomeAssembly, string ManifestPath)> list = Manifest.CreateGenomeAssemblyPaths(ManifestGRCh37, ManifestGRCh38, genomeAssemblies).ToList();
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void CreateGenomeAssemblyPaths_Unknown()
        {
            var genomeAssemblies = new List<GenomeAssembly> { GenomeAssembly.hg19 };
            List<(GenomeAssembly GenomeAssembly, string ManifestPath)> list = Manifest.CreateGenomeAssemblyPaths(ManifestGRCh37, ManifestGRCh38, genomeAssemblies).ToList();
            Assert.Empty(list);
        }
    }
}
