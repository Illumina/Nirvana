using VariantAnnotation.Algorithms;
using Xunit;

namespace UnitTests.VariantAnnotation.Algorithms
{
    public sealed class IntervalOperationsTests
    {
        [Theory]
        [InlineData(1, 10, 5, 6, true)]
        [InlineData(5, 6, 1, 10, true)]
        [InlineData(1, 3, 5, 7, false)]
        [InlineData(5, 7, 1, 3, false)]
        [InlineData(1, 7, 5, 10, true)]
        [InlineData(5, 10, 1, 7, true)]
        public void Overlaps_Theory(int start1, int end1, int start2, int end2, bool expectedResult)
        {
            var observedResult = IntervalUtilities.Overlaps(start1, end1, start2, end2);
            Assert.Equal(expectedResult, observedResult);
        }

        [Theory]
        [InlineData(1, 10, 5, 6, true)]
        [InlineData(5, 6, 1, 10, false)]
        [InlineData(1, 3, 5, 7, false)]
        [InlineData(5, 7, 1, 3, false)]
        [InlineData(1, 7, 5, 10, false)]
        [InlineData(5, 10, 1, 7, false)]
        public void Contains_Theory(int start1, int end1, int start2, int end2, bool expectedResult)
        {
            var observedResult = IntervalUtilities.Contains(start1, end1, start2, end2);
            Assert.Equal(expectedResult, observedResult);
        }
    }
}
