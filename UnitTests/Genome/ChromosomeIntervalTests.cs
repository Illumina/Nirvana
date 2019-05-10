using Genome;
using Xunit;

namespace UnitTests.Genome
{
    public sealed class ChromosomeIntervalTests
    {
        [Fact]
        public void ChromosomeInterval_Setup()
        {
            var expectedChromosome = new Chromosome("chr1", "1", 0);
            const int expectedStart = 100;
            const int expectedEnd = 200;

            var observedInterval = new ChromosomeInterval(expectedChromosome, 100, 200);

            Assert.Equal(expectedChromosome, observedInterval.Chromosome);
            Assert.Equal(expectedStart, observedInterval.Start);
            Assert.Equal(expectedEnd, observedInterval.End);
        }
    }
}
