using Intervals;
using Xunit;

namespace UnitTests.Intervals
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
        public void Overlaps(int start1, int end1, int start2, int end2, bool expectedResult)
        {
            bool observedResult = Utilities.Overlaps(start1, end1, start2, end2);
            Assert.Equal(expectedResult, observedResult);
        }

        [Theory]
        [InlineData(1, 10, 5, 6, true)]
        [InlineData(5, 6, 1, 10, false)]
        [InlineData(1, 3, 5, 7, false)]
        [InlineData(5, 7, 1, 3, false)]
        [InlineData(1, 7, 5, 10, false)]
        [InlineData(5, 10, 1, 7, false)]
        public void Contains(int start1, int end1, int start2, int end2, bool expectedResult)
        {
            bool observedResult = Utilities.Contains(start1, end1, start2, end2);
            Assert.Equal(expectedResult, observedResult);
        }

        [Theory]
        [InlineData(1, 10, 5, 6, 5, 6)]
        [InlineData(5, 6, 1, 10, 5, 6)]
        [InlineData(1, 3, 5, 7, -1, -1)]
        [InlineData(5, 7, 1, 3, -1, -1)]
        [InlineData(1, 7, 5, 10, 5, 7)]
        [InlineData(5, 10, 1, 7, 5, 7)]
        public void Intersects(int start1, int end1, int start2, int end2, int expectedStart, int expectedEnd)
        {
            (int observedStart, int observedEnd) = Utilities.Intersects(start1, end1, start2, end2);
            Assert.Equal(expectedStart, observedStart);
            Assert.Equal(expectedEnd, observedEnd);
        }
    }
}
