using System;
using System.IO;
using Illumina.DataDumperImport.FileHandling;
using NirvanaUnitTests.Utilities;

namespace NirvanaUnitTests.Contexts
{
    public class DataDumperContext : IDisposable
    {
        #region members

        public DataDumperReader Reader;

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
                Reader.Dispose();
            }

            // Free any unmanaged objects here
            _isDisposed = true;
        }

        #endregion

        // constructor
        public DataDumperContext()
        {
            var dumpPath = Path.Combine(Path.GetTempPath(), "refseq-72-chr22-16000001-17000000.gz.dump");

            if (!File.Exists(dumpPath))
            {
                ResourceUtilities.SerializeResource("NirvanaUnitTests.Resources.refseq-72-chr22-16000001-17000000.gz.dump", dumpPath);
            }

            Reader = new DataDumperReader(dumpPath);
        }
    }
}
