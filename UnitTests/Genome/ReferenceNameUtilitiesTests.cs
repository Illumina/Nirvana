using Genome;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.Genome
{
    public sealed class ReferenceNameUtilitiesTests
    {
       [Fact]
        public void GetChromosome_RefName_Exists()
        {
            var chromosome = ReferenceNameUtilities.GetChromosome(ChromosomeUtilities.RefNameToChromosome, "1");
            Assert.Equal(0, chromosome.Index);
        }

        [Fact]
        public void GetChromosome_RefName_DoesNotExist()
        {
            const string chromosomeName = "dummy";
            var chromosome = ReferenceNameUtilities.GetChromosome(ChromosomeUtilities.RefNameToChromosome, chromosomeName);
            Assert.Equal(chromosomeName, chromosome.EnsemblName);
            Assert.Equal(chromosomeName, chromosome.UcscName);
            Assert.True(chromosome.IsEmpty);
        }

        [Fact]
        public void GetChromosome_RefName_NullName()
        {
            var chromosome = ReferenceNameUtilities.GetChromosome(ChromosomeUtilities.RefNameToChromosome, null);
            Assert.Equal(string.Empty, chromosome.EnsemblName);
            Assert.Equal(string.Empty, chromosome.UcscName);
            Assert.True(chromosome.IsEmpty);
        }
    }
}
