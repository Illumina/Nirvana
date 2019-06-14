using System.Collections.Generic;
using System.Linq;
using Downloader;
using Genome;
using Moq;
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

        [Fact]
        public void GetRemotePaths_Nominal()
        {
            var genomeAssemblies = new List<GenomeAssembly> { GenomeAssembly.GRCh37, GenomeAssembly.GRCh38 };

            var expectedPathsGRCh37 = new List<string>
            {
                "0bf0cb93e64824b20f0b551a629596fd-TopMed/3/GRCh37/TOPMed_freeze_5.nsa",
                "43321b1a4f1c73724c00223e07d5e812-1kgSv/3/GRCh37/1000_Genomes_Project_Phase_3_v5a.nsi"
            };

            var expectedPathsGRCh38 = new List<string>
            {
                "645778a7d475ac437d15765ef3c6f50c-OMIM/3/GRCh38/OMIM_20190225.nga"
            };

            var expectedResults = new Dictionary<GenomeAssembly, List<string>>
            {
                [GenomeAssembly.GRCh37] = expectedPathsGRCh37,
                [GenomeAssembly.GRCh38] = expectedPathsGRCh38
            };

            var clientMock = new Mock<IClient>();
            clientMock.Setup(x => x.DownloadLinesAsync(ManifestGRCh37)).ReturnsAsync(expectedPathsGRCh37);
            clientMock.Setup(x => x.DownloadLinesAsync(ManifestGRCh38)).ReturnsAsync(expectedPathsGRCh38);

            Dictionary<GenomeAssembly, List<string>> remotePathsByGenomeAssembly =
                Manifest.GetRemotePaths(clientMock.Object, genomeAssemblies, ManifestGRCh37, ManifestGRCh38);

            Assert.Equal(2, remotePathsByGenomeAssembly.Count);
            Assert.Equal(expectedResults, remotePathsByGenomeAssembly);
        }
    }
}
