using System.Collections;
using System.Collections.Generic;
using UnitTests.Fixtures;
using VariantAnnotation.DataStructures;
using Xunit;

namespace UnitTests.DataStructures
{
    public sealed class IntervalTreeTests : IClassFixture<IntervalTreeContext>
    {
        /// <summary>
        /// get references to our common context
        /// </summary>
        public IntervalTreeTests(IntervalTreeContext context)
        {
            _bob1 = context.Bob1;
            _bob2 = context.Bob2;
            _bob3 = context.Bob3;
            _bob4 = context.Bob4;
            _bob5 = context.Bob5;
            _bob6 = context.Bob6;
            _tree = context.Tree;
        }

        [Fact]
        public void Equality()
        {
            var newBob4 = new IntervalTree<string>.Interval("chr1", 5, 19, "bob4");

            Assert.True(_bob4.Equals(newBob4));
            Assert.Equal(_bob4, newBob4);
            Assert.NotEqual(_bob4, _bob3);
            Assert.True(_bob4 != _bob3);
        }

        [Fact]
        public void FindMultipleOverlaps()
        {
            var positiveOverlaps = new List<string>();
            var negativeOverlaps = new List<string>();

            _tree.GetAllOverlappingValues(new IntervalTree<string>.Interval("chr1", 12, 13), positiveOverlaps);
            _tree.GetAllOverlappingValues(new IntervalTree<string>.Interval("chr2", 22, 23), negativeOverlaps);

            int numPositiveOverlaps = positiveOverlaps.Count;
            int numNegativeOverlaps = negativeOverlaps.Count;

            // compare the results
            Assert.Equal(4, numPositiveOverlaps);
            Assert.Equal(0, numNegativeOverlaps);

            Assert.Contains(_bob1.Values[0], positiveOverlaps);
            Assert.Contains(_bob2.Values[0], positiveOverlaps);
            Assert.Contains(_bob3.Values[0], positiveOverlaps);
            Assert.Contains(_bob4.Values[0], positiveOverlaps);
        }

        [Fact]
        public void Iteration()
        {
            var intervalList = new List<IntervalTree<string>.Interval> {_bob4, _bob5, _bob1, _bob3, _bob2, _bob6};

            int listIndex = 0;
            foreach (var node in _tree)
            {
                Assert.Equal(intervalList[listIndex].ToString(), node.Key.ToString());
                listIndex++;
            }

            // investigate the enumerator
            var enumerator = _tree.GetEnumerator();
            enumerator.MoveNext();
            Assert.Equal(_bob4, enumerator.Current.Key);

            // check the reset
            enumerator.MoveNext();
            enumerator.Reset();
            enumerator.MoveNext();
            Assert.Equal(_bob4, enumerator.Current.Key);

            // check the IEnumerator
            IEnumerator enumerator2 = _tree.GetEnumerator();
            enumerator2.MoveNext();
            var current2 = (IntervalTree<string>.Node) enumerator2.Current;
            Assert.Equal(_bob4, current2.Key);
        }

        #region members

        private static IntervalTree<string> _tree;

        private static IntervalTree<string>.Interval _bob1;
        private static IntervalTree<string>.Interval _bob2;
        private static IntervalTree<string>.Interval _bob3;
        private static IntervalTree<string>.Interval _bob4;
        private static IntervalTree<string>.Interval _bob5;
        private static IntervalTree<string>.Interval _bob6;

        #endregion
    }
}