using System;
using System.IO;
using VariantAnnotation.FileHandling;
using Xunit;

namespace UnitTests.Fixtures
{
    [CollectionDefinition("Chromosome 1 collection")]
    public class ChromosomeOneCollection : ICollectionFixture<ChromosomeOneFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    public class ChromosomeOneFixture : IDisposable
    {
        // constructor
        public ChromosomeOneFixture()
        {
            var annotationLoader = AnnotationLoader.Instance;

            // annotationLoader.DisableSortingCheck();
            annotationLoader.LoadCompressedSequence(Path.Combine("Resources", "Homo_sapiens.GRCh37.75.chr1.Nirvana.dat"));
            annotationLoader.Load("chr1");
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
            }

            // Free any unmanaged objects here
            _isDisposed = true;
        }

        #endregion
    }
}