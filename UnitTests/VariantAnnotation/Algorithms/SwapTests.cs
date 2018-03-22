using VariantAnnotation.Algorithms;
using Xunit;

namespace UnitTests.VariantAnnotation.Algorithms
{
    public sealed class SwapTests
    {
        [Fact]
        public void Swap_Int()
        {
            const int expectedA = 5;
            const int expectedB = 3;

            int observedA = expectedB;
            int observedB = expectedA;
            Swap.Int(ref observedA, ref observedB);

            Assert.Equal(expectedA, observedA);
            Assert.Equal(expectedB, observedB);
        }
    }
}
