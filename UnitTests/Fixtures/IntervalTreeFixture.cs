using System;
using VariantAnnotation.DataStructures;

namespace UnitTests.Fixtures
{
    public class IntervalTreeContext : IDisposable
    {
        // constructor
        public IntervalTreeContext()
        {
            Bob1 = new IntervalTree<string>.Interval("chr1", 10, 20, "bob1");
            Bob2 = new IntervalTree<string>.Interval("chr1", 13, 17, "bob2");
            Bob3 = new IntervalTree<string>.Interval("chr1", 12, 18, "bob3");
            Bob4 = new IntervalTree<string>.Interval("chr1", 5, 19, "bob4");
            Bob5 = new IntervalTree<string>.Interval("chr1", 7, 9, "bob5");
            Bob6 = new IntervalTree<string>.Interval("chr2", 10, 20, "bob6");

            Tree = new IntervalTree<string> {Bob1, Bob2, Bob3, Bob4, Bob5, Bob6};
        }

        #region members

        public readonly IntervalTree<string> Tree;

        public IntervalTree<string>.Interval Bob1;
        public IntervalTree<string>.Interval Bob2;
        public IntervalTree<string>.Interval Bob3;
        public IntervalTree<string>.Interval Bob4;
        public IntervalTree<string>.Interval Bob5;
        public IntervalTree<string>.Interval Bob6;

        #endregion

        #region IDisposable

        private bool _isDisposed;

        /// <summary>
        /// Public implementation of Dispose pattern callable by consumers
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                // Free any other managed objects here
            }

            // Free any unmanaged objects here
            _isDisposed = true;
        }

        #endregion
    }
}