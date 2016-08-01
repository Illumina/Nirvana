using System;
using System.IO;
using VariantAnnotation.FileHandling.Phylop;
using VariantAnnotation.Utilities;

namespace UnitTests.Fixtures
{
    public class PhylopDatabaseFixture : IDisposable
    {
        #region members

        public readonly PhylopReader NpdDatabase;

        #endregion

        // constructor
        public PhylopDatabaseFixture()
        {
            NpdDatabase =
                new PhylopReader(
                    new BinaryReader(
                        FileUtilities.GetFileStream(Path.Combine("Resources", "chr1_10918_150000.npd"))));
        }

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
                // _npdBinaryReader.Close();
            }

            // Free any unmanaged objects here
            _isDisposed = true;
        }

        #endregion
    }
}