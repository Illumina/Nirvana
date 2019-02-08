using System.Collections.Generic;
using System.IO;
using Genome;
using Xunit;

namespace UnitTests.Genome
{
    public sealed class ReferenceNameUtilitiesTests
    {
        private readonly Dictionary<ushort, IChromosome> _refIndexToChromosome = new Dictionary<ushort, IChromosome>();
        private readonly Dictionary<string, IChromosome> _refNameToChromosome  = new Dictionary<string, IChromosome>();

        public ReferenceNameUtilitiesTests()
        {
            var chr1 = new Chromosome("chr1", "1", 0);
            var chr3 = new Chromosome("chr3", "3", 2);
            var chrM = new Chromosome("chrM", "MT", 24);        
            var chromosomes = new List<IChromosome> { chr1, chr3, chrM };

            foreach (var chromosome in chromosomes)
            {
                _refIndexToChromosome[chromosome.Index]      = chromosome;
                _refNameToChromosome[chromosome.UcscName]    = chromosome;
                _refNameToChromosome[chromosome.EnsemblName] = chromosome;
            }
        }

        [Fact]
        public void GetChromosome_RefIndex_Exists()
        {
            var chromosome = ReferenceNameUtilities.GetChromosome(_refIndexToChromosome, 2);
            Assert.Equal("3", chromosome.EnsemblName);
        }

        [Fact]
        public void GetChromosome_RefIndex_DoesNotExist()
        {
            Assert.Throws<InvalidDataException>(delegate
            {
                ReferenceNameUtilities.GetChromosome(_refIndexToChromosome, 1);
            });
        }

        [Fact]
        public void GetChromosome_RefName_Exists()
        {
            var chromosome = ReferenceNameUtilities.GetChromosome(_refNameToChromosome, "1");
            Assert.Equal(0, chromosome.Index);
        }

        [Fact]
        public void GetChromosome_RefName_DoesNotExist()
        {
            const string chromosomeName = "dummy";
            var chromosome = ReferenceNameUtilities.GetChromosome(_refNameToChromosome, chromosomeName);
            Assert.Equal(chromosomeName, chromosome.EnsemblName);
            Assert.Equal(chromosomeName, chromosome.UcscName);
            Assert.True(chromosome.IsEmpty());
        }

        [Fact]
        public void GetChromosome_RefName_NullName()
        {
            var chromosome = ReferenceNameUtilities.GetChromosome(_refNameToChromosome, null);
            Assert.Equal(string.Empty, chromosome.EnsemblName);
            Assert.Equal(string.Empty, chromosome.UcscName);
            Assert.True(chromosome.IsEmpty());
        }
    }
}
