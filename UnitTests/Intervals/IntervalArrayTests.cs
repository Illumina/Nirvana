using System.Collections.Generic;
using System.Linq;
using Cache.Data;
using Intervals;
using Xunit;

namespace UnitTests.Intervals
{
    public sealed class IntervalArrayTests
    {
        private readonly IntervalArray<TranscriptRegion> _intervalArray;

        private static readonly TranscriptRegion Region1 = new(10, 20, 1, 2, TranscriptRegionType.Exon, 1, null);
        private static readonly TranscriptRegion Region2 = new(5, 7, 1, 2, TranscriptRegionType.Exon, 2, null);
        private static readonly TranscriptRegion Region3 = new(7, 9, 1, 2, TranscriptRegionType.Exon, 3, null);

        public IntervalArrayTests()
        {
            TranscriptRegion[] sortedRegions = new[] {Region1, Region2, Region3}
                .OrderBy(x => x.Start)
                .ThenBy(x => x.End)
                .ToArray();

            IntervalArray<TranscriptRegion>.Interval[] sortedIntervals = IntervalUtilities.CreateIntervals(sortedRegions);
            _intervalArray = new IntervalArray<TranscriptRegion>(sortedIntervals);
        }

        public static IEnumerable<object[]> TheoryParameters()
        {
            yield return new object[] {6, 9, new List<TranscriptRegion> {Region2, Region3}};
            yield return new object[] {8, 10, new List<TranscriptRegion> {Region3, Region1}};
            yield return new object[] {11, 50, new List<TranscriptRegion> {Region1}};
            yield return new object[] {21, 23, new List<TranscriptRegion>()};
        }

        [Theory]
        [MemberData(nameof(TheoryParameters))]
        public void AddOverlappingIntervals_ExpectedResults(int begin, int end, List<TranscriptRegion> expected)
        {
            List<TranscriptRegion> actual = new();
            _intervalArray.AddOverlappingValues(actual, begin, end);
            Assert.Equal(expected, actual);
        }
    }
}