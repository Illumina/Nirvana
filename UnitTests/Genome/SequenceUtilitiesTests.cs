using Genome;
using Xunit;

namespace UnitTests.Genome
{
    public sealed class SequenceUtilitiesTests
    {
        [Theory]
        [InlineData("ACGTTTGA", "TCAAACGT")]
        [InlineData(null, null)]
        public void GetReverseComplement(string bases, string expectedResult)
        {
            var observedResult = SequenceUtilities.GetReverseComplement(bases);
            Assert.Equal(expectedResult, observedResult);
        }

        [Theory]
        [InlineData("ACGT", false)]
        [InlineData("ACXT", true)]
        [InlineData(null, false)]
        public void HasNonCanonicalBase(string bases, bool expectedResult)
        {
            var observedResult = SequenceUtilities.HasNonCanonicalBase(bases);
            Assert.Equal(expectedResult, observedResult);
        }
    }
}
