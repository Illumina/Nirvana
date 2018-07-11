using Intervals;
using Xunit;

namespace UnitTests.Intervals
{
    public sealed class IntervalExtensionsTests
    {
        [Theory]
        [InlineData(1, 3, 5, 7, 0, false)]
        [InlineData(1, 3, 5, 7, 2, true)]
        public void Overlaps_TwoIntervalsWithFlankingLength(int start1, int end1, int start2, int end2,
            int flankingLength, bool expectedResult)
        {
            var interval  = new Interval(start1, end1);
            var interval2 = new Interval(start2, end2);
            bool observedResult = interval.Overlaps(interval2, flankingLength);
            Assert.Equal(expectedResult, observedResult);
        }

        [Theory]
        [InlineData(5, 7, 1, 3, false)]
        [InlineData(1, 7, 5, 10, true)]
        public void Overlaps_IntervalAndCoordinates(int start1, int end1, int start2, int end2, bool expectedResult)
        {
            var interval        = new Interval(start1, end1);
            bool observedResult = interval.Overlaps(start2, end2);
            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void Contains_TwoIntervals()
        {
            var interval1 = new Interval(1, 10);
            var interval2 = new Interval(5, 6);
            bool observedResult = interval1.Contains(interval2);
            Assert.True(observedResult);
        }

        [Theory]
        [InlineData(1, 3, 5, 7, -1, -1)]
        [InlineData(1, 7, 5, 7, 5, 7)]
        public void Intersects_TwoIntervals(int start1, int end1, int start2, int end2, int expectedStart,
            int expectedEnd)
        {
            var interval         = new Interval(start1, end1);
            var interval2        = new Interval(start2, end2);
            var observedInterval = interval.Intersects(interval2);
            Assert.Equal(expectedStart, observedInterval.Start);
            Assert.Equal(expectedEnd, observedInterval.End);
        }
    }
}
