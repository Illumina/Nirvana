using System;
using System.Collections.Generic;
using System.Linq;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.Intervals;
using Xunit;

namespace UnitTests.VariantAnnotation.Caches.DataStructures
{
    public sealed class IntervalForestTests
    {
        private readonly IntervalForest<string> _intervalForest;

        public IntervalForestTests()
        {
            var intervalArraysByRefIndex = new IntervalArray<string>[3];
            intervalArraysByRefIndex[0] = GetIntervalArrayRefIndex0();
            intervalArraysByRefIndex[1] = GetIntervalArrayRefIndex1();
            intervalArraysByRefIndex[2] = GetIntervalArrayRefIndex2();
            _intervalForest = new IntervalForest<string>(intervalArraysByRefIndex);
        }

        private static IntervalArray<string> GetIntervalArrayRefIndex0()
        {
            return GetIntervalArray(new List<Interval<string>>
            {
                new Interval<string>(10, 20, "bob"),
                new Interval<string>(5, 7, "mary"),
                new Interval<string>(7, 9, "jane")
            });
        }

        private static IntervalArray<string> GetIntervalArrayRefIndex1()
        {
            return GetIntervalArray(new List<Interval<string>>
            {
                new Interval<string>(100, 200, "jones"),
                new Interval<string>(125, 150, "smith")
            });
        }

        private static IntervalArray<string> GetIntervalArrayRefIndex2()
        {
            return GetIntervalArray(new List<Interval<string>>
            {
                new Interval<string>(9, 28, "zoe"),
                new Interval<string>(1, 7, "clive")
            });
        }

        [Theory]
        [InlineData(0, 4, 4, false)]
        [InlineData(0, 5, 6, true)]
        [InlineData(1, 90, 95, false)]
        [InlineData(2, 5, 6, true)]
        public void OverlapsAny(ushort refIndex, int begin, int end, bool expectedResult)
        {
            Assert.Equal(expectedResult, _intervalForest.OverlapsAny(refIndex, begin, end));
        }

        [Fact]
        public void OverlapsAny_ThrowException_WhenRefIndexInvalid()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var result = _intervalForest.OverlapsAny(3, 10, 20);
            });
        }

        [Theory]
        [InlineData(0, 6, 9, new[] { "mary", "jane" })]
        [InlineData(1, 180, 190, new[] { "jones" })]
        [InlineData(2, 6, 10, new[] { "clive", "zoe" })]
        [InlineData(3, 23, 25, null)]
        public void GetAllOverlappingValues(ushort refIndex, int begin, int end, string[] expectedValues)
        {
            var observedValues = _intervalForest.GetAllOverlappingValues(refIndex, begin, end);
            Assert.Equal(expectedValues, observedValues);
        }

        private static IntervalArray<string> GetIntervalArray(List<Interval<string>> intervals) => new
            IntervalArray<string>(intervals.OrderBy(x => x.Begin).ThenBy(x => x.End).ToArray());
    }
}
