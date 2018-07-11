using Variants;
using Xunit;

namespace UnitTests.Variants
{
    public sealed class BiDirectionalTrimmerTests
    {
        [Theory]
        [InlineData(100, "A", "C", 100, "A", "C")]
        [InlineData(100, "A", "A", 100, "A", "A")]
        [InlineData(100, "AT", null, 100, "AT", "")]
        [InlineData(100, null, "CG", 100, "", "CG")]
        [InlineData(100, "ATTT", "AT", 102, "TT", "")]
        [InlineData(100, "CGGG", "TGGG", 100, "C", "T")]
        public void Trim(int start, string refAllele, string altAllele, int expectedStart, string expectedRef, string expectedAlt)
        {
            (int observedStart, string observedRef, string observedAlt) =
                BiDirectionalTrimmer.Trim(start, refAllele, altAllele);

            Assert.Equal(expectedStart, observedStart);
            Assert.Equal(expectedRef,   observedRef);
            Assert.Equal(expectedAlt,   observedAlt);
        }
    }
}
