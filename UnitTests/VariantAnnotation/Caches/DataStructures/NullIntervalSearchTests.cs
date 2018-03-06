using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.Intervals;
using Xunit;

namespace UnitTests.VariantAnnotation.Caches.DataStructures
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

        [Fact]
        public void GetFirstOverlappingInterval_IIntervalSearch()
        {
            var intervalSearch = new NullIntervalSearch<string>();
            var observedResult = intervalSearch.GetFirstOverlappingInterval(1, 2, out Interval<string> observedValue);
            Assert.False(observedResult);
            Assert.Equal(IntervalArray<string>.EmptyInterval.Begin, observedValue.Begin);
            Assert.Equal(IntervalArray<string>.EmptyInterval.End, observedValue.End);
        }
    }
}
