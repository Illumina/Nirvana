using System;
using System.Collections.Generic;
using SAUtils.DataStructures;

namespace SAUtils.TsvWriters
{
    public class MitoMapMutationTsvWriter : ISaItemTsvWriter
    {

        #region members
        private readonly SaTsvWriter _mitoMapMutWriter;
        #endregion

        #region IDisposable
        bool _disposed;

        /// <summary>
        /// public implementation of Dispose pattern callable by consumers. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void WritePosition(IEnumerable<SupplementaryDataItem> saItems)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// protected implementation of Dispose pattern. 
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                _mitoMapMutWriter.Dispose();
            }

            // Free any unmanaged objects here.
            //
            _disposed = true;
            // Free any other managed objects here.

        }
        #endregion


    }
}