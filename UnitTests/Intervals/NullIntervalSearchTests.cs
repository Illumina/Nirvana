using Intervals;
using Xunit;

namespace UnitTests.Intervals
{
    public sealed class NullIntervalSearchTests
    {
        [Fact]
        public void OverlapsAny_IIntervalForest()
        {
            var intervalForest = new NullIntervalSearch<string>();
            Assert.False(intervalForest.OverlapsAny(1, 2, 3));
        }

        [Fact]
        public void GetAllOverlappingValues_IIntervalForest()
        {
            var intervalForest = new NullIntervalSearch<string>();
            Assert.Null(intervalForest.GetAllOverlappingValues(1, 2, 3));
        }

        [Fact]
        public void GetAllOverlappingValues_IIntervalSearch()
        {
            var intervalSearch = new NullIntervalSearch<string>();
            Assert.Null(intervalSearch.GetAllOverlappingValues(1, 2));
        }
    }
}
