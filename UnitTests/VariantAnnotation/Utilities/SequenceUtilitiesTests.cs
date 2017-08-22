using UnitTests.TestDataStructures;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.VariantAnnotation.Utilities
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

        [Fact]
        public void GetSubSubstring()
        {
            const string expectedResult = "CGTG";
            var sequence = new SimpleSequence("GGTCACACGATTAACCCAAGTCAATAGAAGCCGGCGTAAAGAGTGTTTTAGATCACCCCC");
            var observedResult = SequenceUtilities.GetSubSubstring(4, 10, true, 1, 4, sequence);
            Assert.Equal(expectedResult, observedResult);
        }
    }
}
