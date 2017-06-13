using VariantAnnotation.DataStructures.IntervalSearch;
using VariantAnnotation.Interface;

namespace UnitTests.VariantAnnotationTests.DataStructures
{
    public class ReferenceIntervalArrayTests
    {
        #region members

        private readonly IIntervalSearch<int> _refIntervalSearch;
        
        #endregion

        public ReferenceIntervalArrayTests()
        {
            _refIntervalSearch = GetData();
        }

        private static IIntervalSearch<int> GetData()
        {
            var items = new Interval<int>[7];
            items[0] = new Interval<int>(4, 7, 0);
            items[1] = new Interval<int>(5, 8, 1);
            items[2] = new Interval<int>(10, 28, 2);
            items[3] = new Interval<int>(12, 15, 3);
            items[4] = new Interval<int>(15, 20, 4);
            items[5] = new Interval<int>(17, 19, 5);
            items[6] = new Interval<int>(30, 40, 6);
            return new IntervalArray<int>(items);
        }

        //[Theory]
        //[InlineData(3, 4, true)]
        //[InlineData(6, 7, true)]
        //[InlineData(21, 22, true)]
        //[InlineData(13, 14, true)]
        //[InlineData(31, 33, true)]
        //[InlineData(29, 29, false)]
        //[InlineData(9, 9, false)]
        //public void OverlapsAny(int begin, int end, bool expectedResult)
        //{
        //    var observedResult = _refIntervalSearch.OverlapsAny(begin, end);
        //    Assert.Equal(expectedResult, observedResult);
        //}
    }
}
