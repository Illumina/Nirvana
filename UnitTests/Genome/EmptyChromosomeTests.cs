using Genome;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.Genome
{
    public sealed class EmptyChromosomeTests
    {
        private readonly IChromosome _emptyChromosome  = new EmptyChromosome("chr1");
        private readonly IChromosome _emptyChromosome2 = new EmptyChromosome("chr1");

        [Fact]
        public void Equals_True()
        {
            Assert.True(_emptyChromosome.Equals(_emptyChromosome2));
        }

        [Fact]
        public void Equals_False()
        {
            Assert.False(_emptyChromosome.Equals(ChromosomeUtilities.Chr1));
            Assert.False(ChromosomeUtilities.Chr1.Equals(_emptyChromosome));
        }

        [Fact]
        public void GetHashCode_True()
        {
            Assert.Equal(_emptyChromosome.GetHashCode(), _emptyChromosome2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_False()
        {
            Assert.NotEqual(_emptyChromosome.GetHashCode(), ChromosomeUtilities.Chr1.GetHashCode());
        }
    }
}
