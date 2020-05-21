using RepeatExpansions;
using Xunit;

namespace UnitTests.RepeatExpansions
{
    public sealed class PercentileUtilitiesTests
    {
        private readonly int[] _values = { 7, 8, 9, 10, 11, 12, 13, 15 };
        private readonly double[] _percentiles  = { 0, 1, 1.5, 3.5, 75.5, 86.5, 98.5, 99.5 };

        [Fact]
        public void ComputePercentiles_Nominal()
        {
            var repeatNumbers = new[] { 10, 15, 20, 100, 200 };
            var alleleCounts = new[] { 550, 34, 78, 30, 45 };

            double[] expectedPercentiles = {
                0, 550 * 100.0 / 737, (550 + 34) * 100.0 / 737, (550 + 34 + 78) * 100.0 / 737,
                (550 + 34 + 78 + 30) * 100.0 / 737
            };

            double[] observedResults = PercentileUtilities.ComputePercentiles(repeatNumbers.Length, alleleCounts);
            Assert.Equal(expectedPercentiles, observedResults);
        }

        [Fact]
        public void GetPercentile_RepeatNumberInRange_PositiveIndex()
        {
            double observedResult = PercentileUtilities.GetPercentile(14, _values, _percentiles);
            Assert.Equal(99.5, observedResult);
        }

        [Fact]
        public void GetPercentile_RepeatNumberOutOfRange_NegativeIndex()
        {
            double observedResult = PercentileUtilities.GetPercentile(20, _values, _percentiles);
            Assert.Equal(100, observedResult);
        }
    }
}
