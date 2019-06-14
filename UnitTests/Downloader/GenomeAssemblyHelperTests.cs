using System.Collections.Generic;
using ErrorHandling.Exceptions;
using Genome;
using Xunit;
using GenomeAssemblyHelper = Downloader.GenomeAssemblyHelper;

namespace UnitTests.Downloader
{
    public sealed class GenomeAssemblyHelperTests
    {
        [Fact]
        public void GetGenomeAssemblies_GRCh37()
        {
            List<GenomeAssembly> genomeAssemblies = GenomeAssemblyHelper.GetGenomeAssemblies("GRCh37");
            Assert.Single(genomeAssemblies);
            Assert.Equal(GenomeAssembly.GRCh37, genomeAssemblies[0]);
        }

        [Fact]
        public void GetGenomeAssemblies_GRCh38()
        {
            List<GenomeAssembly> genomeAssemblies = GenomeAssemblyHelper.GetGenomeAssemblies("GrcH38");
            Assert.Single(genomeAssemblies);
            Assert.Equal(GenomeAssembly.GRCh38, genomeAssemblies[0]);
        }

        [Fact]
        public void GetGenomeAssemblies_Both()
        {
            List<GenomeAssembly> genomeAssemblies = GenomeAssemblyHelper.GetGenomeAssemblies("BoTh");
            Assert.Equal(2, genomeAssemblies.Count);
            Assert.Equal(GenomeAssembly.GRCh37, genomeAssemblies[0]);
            Assert.Equal(GenomeAssembly.GRCh38, genomeAssemblies[1]);
        }

        [Fact]
        public void GetGenomeAssemblies_Unknown()
        {
            Assert.Throws<UserErrorException>(delegate
            {
                // ReSharper disable once UnusedVariable
                List<GenomeAssembly> genomeAssemblies = GenomeAssemblyHelper.GetGenomeAssemblies("hg19");
            });
        }
    }
}
