using System;
using System.Collections.Generic;
using System.IO;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Utilities;
using ErrorHandling.Exceptions;

namespace VariantAnnotation.FileHandling.CustomInterval
{
    public sealed class CustomIntervalReader : IDisposable
    {
        private readonly ExtendedBinaryReader _reader;
        private string _referenceName;
        private string _intervalType;
        // ReSharper disable once NotAccessedField.Local
        private long _creationTime;

        private bool _reachedEnd;

        public DataSourceVersion DataVersion { get; private set; }

        public CustomIntervalReader(string fileName)
        {
            _reader = new ExtendedBinaryReader(FileUtilities.GetReadStream(fileName));
            _reachedEnd = false;

            ReadHeader();
        }

        public string GetIntervalType()
        {
            return _intervalType;
        }

        public string GetReferenceName()
        {
            return _referenceName;
        }

        public CustomIntervalReader(Stream stream)
        {
            _reader = new ExtendedBinaryReader(stream);
            _reachedEnd = false;

            ReadHeader();
        }

        public DataStructures.CustomInterval GetNextCustomInterval()
        {
            if (_reachedEnd) return null;

            var chromosome = _referenceName;
            var type = _intervalType;
            var start = _reader.ReadOptInt32();
            var end = _reader.ReadOptInt32();

            var interval = new DataStructures.CustomInterval(chromosome, start, end, type, null, null);
            if (interval.IsEmpty())
            {
                _reachedEnd = true;
                return null;
            }

            var stringDictCount = _reader.ReadOptInt32();
            if (stringDictCount > 0)
            {
                interval.StringValues = new Dictionary<string, string>(stringDictCount);
                for (var i = 0; i < stringDictCount; i++)
                {
                    var key = _reader.ReadUtf8String();
                    var val = _reader.ReadUtf8String();

                    interval.StringValues.Add(key, val);
                }
            }

            var nonStringDictCount = _reader.ReadOptInt32();
            if (nonStringDictCount > 0)
            {
                interval.NonStringValues = new Dictionary<string, string>(nonStringDictCount);
                for (var i = 0; i < nonStringDictCount; i++)
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
            var header = _reader.ReadString();
            if (header != CustomIntervalCommon.DataHeader)
                throw new GeneralException("Unrecognized header in custom interval database");

            var schema = _reader.ReadUInt16();
            if (schema != CustomIntervalCommon.SchemaVersion)
                throw new GeneralException(
                    $"Custom interval database schema mismatch. Expected {CustomIntervalCommon.SchemaVersion}, observed {schema}");

            _creationTime = _reader.ReadInt64();
            _referenceName = _reader.ReadString();
            _intervalType = _reader.ReadString();

            DataVersion = new DataSourceVersion(_reader);

            CheckGuard();
        }

        private void CheckGuard()
        {
            var observedGuard = _reader.ReadUInt32();
            if (observedGuard != CustomIntervalCommon.GuardInt)
            {
                throw new GeneralException($"Expected a guard integer ({CustomIntervalCommon.GuardInt}), but found another value: ({observedGuard})");
            }
        }

        private static void Close()
        {
            //Console.WriteLine("Read {0} intervals for {1}", _count, _referenceName);
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
                    _reader.Dispose();
                }

                // Free any unmanaged objects here. 
                _isDisposed = true;
            }
        }

        #endregion

    }
}
