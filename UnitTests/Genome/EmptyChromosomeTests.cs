using Genome;
using Xunit;

namespace UnitTests.Genome
{
    public sealed class EmptyChromosomeTests
    {
        private readonly IChromosome _emptyChromosome  = new EmptyChromosome("chr1");
        private readonly IChromosome _emptyChromosome2 = new EmptyChromosome("chr1");
        private readonly IChromosome _chr1             = new Chromosome("chr1", "1", 0);

        [Fact]
        public void Equals_True()
        {
            Assert.True(_emptyChromosome.Equals(_emptyChromosome2));
        }

        [Fact]
        public void Equals_False()
        {
            Assert.False(_emptyChromosome.Equals(_chr1));
            Assert.False(_chr1.Equals(_emptyChromosome));
        }

        [Fact]
        public void GetHashCode_True()
        {
            Assert.Equal(_emptyChromosome.GetHashCode(), _emptyChromosome2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_False()
        {
            Assert.NotEqual(_emptyChromosome.GetHashCode(), _chr1.GetHashCode());
        }
    }
}
