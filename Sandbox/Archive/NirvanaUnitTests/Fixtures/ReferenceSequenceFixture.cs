using System;
using Illumina.VariantAnnotation.FileHandling;
using Xunit;

namespace NirvanaUnitTests.Fixtures
{
    [CollectionDefinition("GRCh37 collection")]
    public class Grch37Collection : ICollectionFixture<Grch37Fixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    public class Grch37Fixture : IDisposable
    {
        // constructor
        public Grch37Fixture()
        {
            var annotationLoader = AnnotationLoader.Instance;

            annotationLoader.LoadCompressedSequence(@"E:\Data\Nirvana\References\Homo_sapiens.GRCh37.75.Nirvana.dat");
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