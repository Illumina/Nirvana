using Intervals;
using Xunit;

namespace UnitTests.Intervals
{
    public sealed class OverlapTypeTests
    {
        // given two intervals T and V, describe how V overlaps T
        [Theory]
        [InlineData(400, 500, OverlapType.Partial)]
        [InlineData(200, 400, OverlapType.CompletelyWithin)]
        [InlineData(100, 200, OverlapType.Partial)]
        [InlineData(100, 500, OverlapType.CompletelyOverlaps)]
        [InlineData(200, 500, OverlapType.CompletelyOverlaps)]
        [InlineData(100, 400, OverlapType.CompletelyOverlaps)]
        [InlineData(500, 600, OverlapType.None)]
        [InlineData(0,   100, OverlapType.None)]
        public void GetOverlapType(int vStart, int vEnd, OverlapType expectedResults)
        {
            const int tStart = 200;
            const int tEnd   = 400;

            OverlapType observedResults = Utilities.GetOverlapType(tStart, tEnd, vStart, vEnd);

            Assert.Equal(expectedResults, observedResults);
        }
    }
}