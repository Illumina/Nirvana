using RepeatExpansions;
using Xunit;

namespace UnitTests.RepeatExpansions
{
    public sealed class RepeatExpansionPhenotypeTests
    {
        private readonly RepeatExpansionPhenotype _phenotype;

        public RepeatExpansionPhenotypeTests()
        {
            var repeatNumbers    = new[] { 7, 8, 9, 10, 11, 12, 13, 15 };
            double[] percentiles = { 0, 1, 1.5, 3.5, 75.5, 86.5, 98.5, 99.5 };

            _phenotype = new RepeatExpansionPhenotype(null, null, null, repeatNumbers, percentiles, null, null);
        }

        [Fact]
        public void GetPercentile_RepeatNumberInRange_PositiveIndex()
        {
            double observedResult = _phenotype.GetPercentile(14);
            Assert.Equal(99.5, observedResult);
        }

        [Fact]
        public void GetPercentile_RepeatNumberOutOfRange_NegativeIndex()
        {
            double observedResult = _phenotype.GetPercentile(20);
            Assert.Equal(100, observedResult);
        }
    }
}
