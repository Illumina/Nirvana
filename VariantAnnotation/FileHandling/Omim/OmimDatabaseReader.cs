using System;
using System.Collections.Generic;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Utilities;
using ErrorHandling.Exceptions;

namespace VariantAnnotation.FileHandling.Omim
{
    public sealed class OmimDatabaseReader : IDisposable
    {
        private readonly ExtendedBinaryReader _reader;
        private readonly string _omimFile;
        // ReSharper disable once NotAccessedField.Local
        private long _creationTime;

        public DataSourceVersion DataVersion { get; private set; }
        private bool _isDisposed;

        public OmimDatabaseReader(string omimDatabaseFile)
        {

            // open the database file
            _omimFile = omimDatabaseFile;
            _reader = new ExtendedBinaryReader(FileUtilities.GetReadStream(omimDatabaseFile));

            ReadHeader();
        }

        private void ReadHeader()
        {
            var header = _reader.ReadString();
            if (header != OmimDatabaseCommon.DataHeader)
                throw new GeneralException("Unrecognized header in OMIM database");

            var schema = _reader.ReadUInt16();
            if (schema != OmimDatabaseCommon.SchemaVersion)
                throw new GeneralException(
                    $"Custom interval database schema mismatch. Expected {OmimDatabaseCommon.SchemaVersion}, observed {schema}");

            _creationTime = _reader.ReadInt64();


            DataVersion = new DataSourceVersion(_reader);

            CheckGuard();
        }

        public IEnumerable<OmimAnnotation> Read()
        {

            var count = _reader.ReadOptInt32();

            for (var i = 0; i < count; i++)
            {
                var omim = OmimAnnotation.Read(_reader);
                yield return omim;
            }
            // check the footer
            CheckFooter();

        }

        /// <summary>
        /// checks if the footer is good
        /// </summary>
        private void CheckFooter()
        {
            const string expectedFooter = "EOF";
            var footer = _reader.ReadAsciiString();

            if (footer != expectedFooter)
            {
                throw new GeneralException($"The footer check failed for the OMIM databses ({_omimFile}): ID: exp: {expectedFooter} obs: {footer}");
            }
        }

        private void CheckGuard()
        {
            var observedGuard = _reader.ReadUInt32();
            if (observedGuard != OmimDatabaseCommon.GuardInt)
            {
                throw new GeneralException($"Expected a guard integer ({OmimDatabaseCommon.GuardInt}), but found another value: ({observedGuard})");
            }
        }

        /// <summary>
        /// public implementation of Dispose pattern callable by consumers. 
        /// </summary>
        public void Dispose()
        {
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
                    _reader.Dispose();
                }

                // Free any unmanaged objects here. 
                _isDisposed = true;
            }
        }
    }
}