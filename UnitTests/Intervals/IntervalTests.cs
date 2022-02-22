using Intervals;
using Xunit;

namespace UnitTests.Intervals
{
    public sealed class IntervalTests
    {
        [Theory]
        [InlineData(9, false)]
        [InlineData(10, true)]
        [InlineData(11, true)]
        [InlineData(20, true)]
        public void Contains(int position, bool expectedResult)
        {
            var  interval       = new IntervalArray<int>.Interval(10, 20, 3, -1);
            bool observedResult = interval.Contains(position);
            Assert.Equal(expectedResult, observedResult);
        }
    }
}
