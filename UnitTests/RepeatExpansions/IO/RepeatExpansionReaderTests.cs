using RepeatExpansions.IO;
using Xunit;

namespace UnitTests.RepeatExpansions.IO
{
    public sealed class RepeatExpansionReaderTests
    {
        [Fact]
        public void ComputePercentiles_Nominal()
        {
            var repeatNumbers = new[] { 10, 15, 20, 100, 200 };
            var alleleCounts  = new[] { 550, 34, 78, 30, 45 };

            double[] expectedPercentiles = {
                0, 550 * 100.0 / 737, (550 + 34) * 100.0 / 737, (550 + 34 + 78) * 100.0 / 737,
                (550 + 34 + 78 + 30) * 100.0 / 737
            };

            double[] observedResults = RepeatExpansionReader.ComputePercentiles(repeatNumbers, alleleCounts);
            Assert.Equal(expectedPercentiles, observedResults);
        }
    }
}
