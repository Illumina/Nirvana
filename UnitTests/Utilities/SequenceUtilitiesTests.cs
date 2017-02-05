using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.Utilities
{
    public sealed class SequenceUtilitiesTests
    {
        [Theory]
        [InlineData("ACGT-")]
        [InlineData("CCCCCC")]
        public void Canonical(string bases)
        {
            Assert.False(SequenceUtilities.HasNonCanonicalBase(bases));
        }

        [Theory]
        [InlineData("BOB")]
        [InlineData("ACGTNX-")]
        public void NonCanonical(string bases)
        {
            Assert.True(SequenceUtilities.HasNonCanonicalBase(bases));
        }
    }
}