using System.Collections.Generic;
using System.Linq;
using Intervals;
using Xunit;

namespace UnitTests.Intervals
{
    public sealed class IntervalArrayTests
    {
        private readonly IntervalArray<string> _intervalArray;

        public IntervalArrayTests()
        {
            var intervals = new List<Interval<string>>
            {
                new Interval<string>(10, 20, "bob"),
                new Interval<string>(5, 7, "mary"),
                new Interval<string>(7, 9, "jane")
            };

            // interval array expects a sorted array of intervals
            _intervalArray = new IntervalArray<string>(intervals.OrderBy(x => x.Begin).ThenBy(x => x.End).ToArray());
        }

        [Theory]
        [InlineData(4, 4, false)]
        [InlineData(5, 6, true)]
        [InlineData(7, 11, true)]
        [InlineData(21, 23, false)]
        public void OverlapsAny(int begin, int end, bool expectedResult)
        {
            Assert.Equal(expectedResult, _intervalArray.OverlapsAny(begin, end));
        }

        [Theory]
        [InlineData(6, 9, new[] { "mary", "jane" })]
        [InlineData(8, 10, new[] { "jane", "bob" })]
        [InlineData(11, 50, new[] { "bob" })]
        [InlineData(21, 23, null)]
        public void GetAllOverlappingValues(int begin, int end, string[] expectedValues)
        {
            var observedValues = _intervalArray.GetAllOverlappingValues(begin, end);
            Assert.Equal(expectedValues, observedValues);
        }
    }
}
