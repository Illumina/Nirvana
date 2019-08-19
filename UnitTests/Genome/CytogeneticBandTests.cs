using Genome;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.Genome
{
    public sealed class CytogeneticBandTests
    {
        private static readonly Band[] CytogeneticBands = {
            new Band(88300001, 92800000, "q14.3"),
            new Band(92800001, 97200000, "q21")
        };

        [Theory]
        [InlineData(88400000, 92900000, "11q14.3-q21")]
        [InlineData(88400000, 92400000, "11q14.3")]
        [InlineData(92820001, 92900000, "11q21")]
        [InlineData(92820001, 92820001, "11q21")]
        [InlineData(1, 1, null)]
        [InlineData(97000000, 98200000, null)]
        public void GetCytogeneticBand_Range(int start, int end, string expectedCytogeneticBand)
        {
            string observedCytogeneticBand = CytogeneticBands.Find(ChromosomeUtilities.Chr11, start, end);

            Assert.Equal(expectedCytogeneticBand, observedCytogeneticBand);
        }

        [Fact]
        public void GetCytogeneticBand_UnknownReference_ReturnNull()
        {
            string observedCytogeneticBand = CytogeneticBands.Find(ChromosomeUtilities.Chr12, 100, 200);
            Assert.Null(observedCytogeneticBand);
        }

        [Fact]
        public void GetCytogeneticBand_UnknownReferenceIndex_ReturnNull()
        {
            string observedCytogeneticBand = CytogeneticBands.Find(ChromosomeUtilities.Bob, 100, 200);
            Assert.Null(observedCytogeneticBand);
        }
    }
}
