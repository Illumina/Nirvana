using Genome;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.Genome
{
    public sealed class ChromosomeIntervalTests
    {
        [Fact]
        public void ChromosomeInterval_Setup()
        {
            const int expectedStart = 100;
            const int expectedEnd = 200;

            var observedInterval = new ChromosomeInterval(ChromosomeUtilities.Chr1, 100, 200);

            Assert.Equal(ChromosomeUtilities.Chr1, observedInterval.Chromosome);
            Assert.Equal(expectedStart, observedInterval.Start);
            Assert.Equal(expectedEnd, observedInterval.End);
        }
    }
}
