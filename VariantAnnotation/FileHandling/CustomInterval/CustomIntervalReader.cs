using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.FileHandling.CustomInterval
{
    public sealed class CustomIntervalReader : IDisposable
    {
        private readonly BinaryReader _binaryReader;
        private readonly ExtendedBinaryReader _reader;
        private string _referenceName;
        private string _intervalType;
        // ReSharper disable once NotAccessedField.Local
        private long _creationTime;
        private int _count = 0;

        private bool _reachedEnd;

        /// <summary>
        /// constructor
        /// </summary>
        public CustomIntervalReader(string fileName)
            : this(FileUtilities.GetFileStream(fileName))
        { }

        /// <summary>
        /// constructor
        /// </summary>
        private CustomIntervalReader(Stream stream)
        {
            // open the database file
            _binaryReader = new BinaryReader(stream);
            _reader       = new ExtendedBinaryReader(_binaryReader);
            _reachedEnd   = false;

            ReadHeader();
        }

        public DataStructures.CustomInterval GetNextCustomInterval()
        {
            if (_reachedEnd) return null;

            var chromosome = _referenceName;
            var type = _intervalType;
            var start = _reader.ReadInt();
            var end = _reader.ReadInt();

            var interval = new DataStructures.CustomInterval(chromosome, start, end, type, null, null);
            if (interval.IsEmpty())
            {
                _reachedEnd = true;
                return null;
            }

            var stringDictCount = _reader.ReadInt();
            if (stringDictCount > 0)
            {
                interval.StringValues = new Dictionary<string, string>(stringDictCount);
                for (int i = 0; i < stringDictCount; i++)
                {
                    var key = _reader.ReadUtf8String();
                    var val = _reader.ReadUtf8String();

                    interval.StringValues.Add(key, val);
                }

            }

            var nonStringDictCount = _reader.ReadInt();
            if (nonStringDictCount > 0)
            {
                interval.NonStringValues = new Dictionary<string, string>(nonStringDictCount);
                for (int i = 0; i < nonStringDictCount; i++)
                {
                    var key = _reader.ReadUtf8String();
                    var val = _reader.ReadUtf8String();

                    interval.NonStringValues.Add(key, val);
                }
            }

            return interval;

        }

        private void ReadHeader()
        {
            var header = _binaryReader.ReadString();
            if (header != CustomIntervalCommon.DataHeader)
            {
                throw new GeneralException("Unrecognized header in custom interval database");
            }

            var schema = _binaryReader.ReadUInt16();
            if (schema != CustomIntervalCommon.SchemaVersion)
            {
                throw new GeneralException($"Custom interval database schema mismatch. Expected {CustomIntervalCommon.SchemaVersion}, observed {schema}");
            }

            _creationTime  = _binaryReader.ReadInt64();
            _referenceName = _binaryReader.ReadString();
            _intervalType  = _binaryReader.ReadString();

            // ReSharper disable once UnusedVariable
            var dataVersion = new DataSourceVersion(_binaryReader);

            CheckGuard();
        }

        private void CheckGuard()
        {
            uint observedGuard = _binaryReader.ReadUInt32();
            if (observedGuard != CustomIntervalCommon.GuardInt)
            {
                throw new GeneralException($"Expected a guard integer ({CustomIntervalCommon.GuardInt}), but found another value: ({observedGuard})");
            }
        }

        private void Close()
        {
            Console.WriteLine("Read {0} intervals for {1}", _count, _referenceName);
        }

        #region IDisposable

        private bool _isDisposed;


        /// <summary>
        /// public implementation of Dispose pattern callable by consumers. 
        /// </summary>
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
                    _binaryReader.Dispose();
                }

                // Free any unmanaged objects here. 
                _isDisposed = true;
            }
        }

        #endregion

    }
}
