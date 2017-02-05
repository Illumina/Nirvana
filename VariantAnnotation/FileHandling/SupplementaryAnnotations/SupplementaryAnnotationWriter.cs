using System;
using System.Collections.Generic;
using System.IO;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.FileHandling.SupplementaryAnnotations
{
    public sealed class SupplementaryAnnotationWriter : IDisposable
    {
        #region members

        private readonly ExtendedBinaryWriter _writer;
        private FileStream _stream;

        private long _dataSourceVersionsOffset;
        private long _dataOffset;
        private long _eofOffset;

        private long _offsetHeader;

        private readonly string _saPath;
        private readonly string _currentRefSeq;
        private readonly GenomeAssembly _currentGenomeAssembly;

        private readonly List<DataSourceVersion> _dataSourceVersions;
        private List<SupplementaryInterval> _supplementaryIntervals;

        private SaIndex _index;
        public int RefMinorCount;

        private long _intervalOffset;

        #endregion

        #region IDisposable


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
            if (disposing)
            {
                // write a negative length
                _writer.WriteOpt(-1);


                // write the intervals
                WriteSupplementaryIntervals();

                // write the guard integer
                _writer.Flush();
                _eofOffset = _stream.Position;
                _writer.Write(SupplementaryAnnotationCommon.GuardInt);

                // update the offsets in the header
                _stream.Position = _offsetHeader;
                _writer.Write(_dataSourceVersionsOffset);
                _writer.Write(_dataOffset);
                _writer.Write(_intervalOffset);
                _writer.Write(_eofOffset);

                // close the file streams
                _writer.Dispose();
                _stream.Dispose();
                _index.Write(_saPath + ".idx", _currentRefSeq);
            }

            // reset all the class variables
            _stream = null;

            _dataSourceVersionsOffset = 0;
            _dataOffset = 0;
            _eofOffset = 0;
            _offsetHeader = 0;
            _intervalOffset = 0;

            _index = null;
        }

        #endregion

        // constructor
        public SupplementaryAnnotationWriter(string saPath, string currentRefSeq, List<DataSourceVersion> dataSourceVersions, GenomeAssembly genomeAssembly = GenomeAssembly.Unknown)
        {
            _dataSourceVersions = dataSourceVersions;
            _saPath = saPath;
            _currentRefSeq = currentRefSeq;
            _currentGenomeAssembly = genomeAssembly;

            _stream = FileUtilities.GetCreateStream(saPath);
            _writer = new ExtendedBinaryWriter(_stream);
            _index = new SaIndex();

            WriteHeader();
        }


        public void SetIntervalList(List<SupplementaryInterval> intervalList)
        {
            _supplementaryIntervals = intervalList;
        }


        private void WriteSupplementaryIntervals()
        {
            _writer.Flush();
            if (null == _supplementaryIntervals)
            {
                _intervalOffset = -1;
                return;
            }

            if (_supplementaryIntervals.Count == 0)
            {
                _intervalOffset = -1;
                return;
            }

            _intervalOffset = _stream.Position;
            _writer.Write(_supplementaryIntervals.Count);

            foreach (var interval in _supplementaryIntervals)
            {
                interval.Write(_writer);
            }

            _supplementaryIntervals?.Clear();
        }

        /// <summary>
        /// writes the annotations to the current database file
        /// </summary>
        public void Write(SupplementaryPositionCreator spCreator, int referencePos, bool finalizePositinalAnnotation = true)
        {
            if (finalizePositinalAnnotation)
                spCreator.FinalizePositionalAnnotations();

            if (spCreator.IsEmpty())
                return;

            // add this entry to the index
            var currentOffset = _stream.Position;
            _index.Add((uint)referencePos, (uint)currentOffset, spCreator.IsRefMinor());
            if (spCreator.IsRefMinor()) RefMinorCount++;

            spCreator.WriteAnnotation(_writer);
        }


        /// <summary>
        /// writes the header to the current database file
        /// </summary>
        private void WriteHeader()
        {
            _writer.Write(System.Text.Encoding.ASCII.GetBytes(SupplementaryAnnotationCommon.DataHeader));
            _writer.Write(SupplementaryAnnotationCommon.DataVersion);
            _writer.Write(SupplementaryAnnotationCommon.SchemaVersion);
            _writer.Write((byte)_currentGenomeAssembly);
            _writer.Write(DateTime.UtcNow.Ticks);
            _writer.Write(_currentRefSeq);

            // reserve space for the offsets
            _writer.Flush();
            _offsetHeader = _stream.Position;
            _writer.Write(_dataSourceVersionsOffset);
            _writer.Write(_dataOffset);
            _writer.Write(_intervalOffset);
            _writer.Write(_eofOffset);

            // write the data source versions
            _writer.Flush();
            _dataSourceVersionsOffset = _stream.Position;

            var numDataSourceVersions = _dataSourceVersions?.Count ?? 0;
            _writer.WriteOpt(numDataSourceVersions);

            if (_dataSourceVersions != null)
            {
                foreach (var dataSourceVersion in _dataSourceVersions) dataSourceVersion.Write(_writer);
            }

            // grab the data offset
            _writer.Flush();
            _dataOffset = _stream.Position;
        }
    }
}
