using System;
using System.Collections.Generic;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling.Binary;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.FileHandling.Omim
{
    public sealed class OmimDatabaseWriter: IDisposable
    {
        #region members
        private readonly ExtendedBinaryWriter _writer;
        private readonly DataSourceVersion _version;
        private int _count;

        private bool _isDisposed;//for dispose pattern

        #endregion

        public OmimDatabaseWriter(string fileName, DataSourceVersion version)
        {
            var stream = FileUtilities.GetCreateStream(fileName);
            _writer = new ExtendedBinaryWriter(stream);
            _version = version;

            WriteHeader();
        }

        private void WriteHeader()
        {
            _writer.Write(OmimDatabaseCommon.DataHeader);
            _writer.Write(OmimDatabaseCommon.SchemaVersion);
            _writer.Write(DateTime.UtcNow.Ticks);


            _version.Write(_writer);
            _writer.Write(OmimDatabaseCommon.GuardInt);

        }

        private void WriteFooter()
        {
            _writer.WriteOptAscii("EOF");
        }

        public void WriteOmims(List<OmimAnnotation> entries)
        {
            _writer.WriteOpt(entries.Count);
            foreach (var entry in entries)
            {
                entry.Write(_writer);
                _count++;
            }
            
        }
        private void Close()
        {
            WriteFooter();
            Console.WriteLine($"Wrote {_count} Omim gene entries");

        }

        public void Dispose()
        {
            Close();
            Dispose(true);
        }

        /// <summary>
        /// protected implementation of Dispose pattern. 
        /// </summary>
        private void Dispose(bool disposing)
        {
            lock (this)
            {
                if (_isDisposed) return;

                if (disposing)
                {
                    // Free any other managed objects here. 
                    _writer.Dispose();
                }

                // Free any unmanaged objects here. 
                _isDisposed = true;
            }
        }
    }
}