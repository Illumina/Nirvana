using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.IntervalSearch;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.FileHandling.SupplementaryAnnotations
{
    public sealed class SupplementaryAnnotationReader : IDisposable, ISupplementaryAnnotationReader
    {
        #region members

        private ExtendedBinaryReader _reader;
        private Stream _stream;
        private Stream _idxStream;
        public readonly SupplementaryAnnotationHeader Header;

        private SaIndex _index;

        private readonly long _intervalsPosition;

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
                _stream.Dispose();
                _idxStream.Dispose();
            }

            _reader     = null;
            _stream     = null;
            _idxStream  = null;
            _index      = null;
        }

        #endregion

        // constructor
        public SupplementaryAnnotationReader(string saPath)
            : this(FileUtilities.GetReadStream(saPath), FileUtilities.GetReadStream(saPath + ".idx"), saPath)
        {}

        // constructor
        public SupplementaryAnnotationReader(Stream dbStream, Stream idxStream, string saPath = null)
        {
            // open the database file
            _stream    = dbStream;
            _idxStream = idxStream;
            _reader    = new ExtendedBinaryReader(_stream);
            _index     = new SaIndex(new ExtendedBinaryReader(idxStream));

            // check the header
            Header = GetHeader(_reader, out _intervalsPosition, saPath);
        }

        /// <summary>
        /// finds the positional record that starts at the specified position
        /// </summary>
        public ISupplementaryAnnotationPosition GetAnnotation(int referencePos)
        {
            // grab the index file offset
            long fileOffset = _index.GetFileLocation((uint)referencePos);
            if (fileOffset == uint.MinValue) return null;

            // read the position
            _stream.Position = fileOffset;

            // read the rest of the supplementary annotation
            return Read(_reader, referencePos);
        }

        /// <summary>
        /// returns the header from the specified Nirvana database file
        /// </summary>
        public static SupplementaryAnnotationHeader GetHeader(string saPath, out long intervalsPosition)
        {
            SupplementaryAnnotationHeader header;

            using (var reader = new ExtendedBinaryReader(FileUtilities.GetReadStream(saPath)))
            {
                header = GetHeader(reader, out intervalsPosition, saPath);
            }

            return header;
        }

        /// <summary>
        /// checks if the header is good
        /// </summary>
        private static SupplementaryAnnotationHeader GetHeader(ExtendedBinaryReader reader, out long intervalsPosition, string saPath = null)
        {
            // check the header and data version
            var header         = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(SupplementaryAnnotationCommon.DataHeader.Length));
            var dataVersion    = reader.ReadUInt16();
            var schemaVersion  = reader.ReadUInt16();
            var genomeAssembly = (GenomeAssembly)reader.ReadByte();

            if (header != SupplementaryAnnotationCommon.DataHeader || schemaVersion != SupplementaryAnnotationCommon.SchemaVersion)
            {
                throw new UserErrorException($"The header check failed for the supplementary annotation file ({saPath ?? "(resource)"}): ID: exp: {SupplementaryAnnotationCommon.DataHeader} obs: {header}, schema version: exp:{SupplementaryAnnotationCommon.SchemaVersion} obs: {schemaVersion}");
            }

            var creationTimeTicks     = reader.ReadInt64();
            var referenceSequenceName = reader.ReadString();

            // skip over the offsets since they're not currently used
            reader.ReadInt64(); // _dataSourceVersionsOffset
            reader.ReadInt64(); // _dataOffset
            intervalsPosition = reader.ReadInt64();
            reader.ReadInt64(); // _eofOffset

            // load the data source versions
            var numDataSourceVersions = reader.ReadOptInt32();
            var dataSourceVersions    = new List<DataSourceVersion>();

            for (var i = 0; i < numDataSourceVersions; i++) dataSourceVersions.Add(DataSourceVersion.Read(reader));

            return new SupplementaryAnnotationHeader(referenceSequenceName, creationTimeTicks, dataVersion,
                dataSourceVersions, genomeAssembly);
        }

        public IEnumerable<ISupplementaryInterval> GetSupplementaryIntervals(IChromosomeRenamer renamer)
        {
            if (_intervalsPosition == -1) return null;
            var returnPosition = _stream.Position;
            _stream.Position = _intervalsPosition;

            var count = _reader.ReadInt32(); // how many supplementary intervals to read

            if (count == 0) return null;

            var intervalList = new List<SupplementaryInterval>(count);
            for (var i = 0; i < count; i++)
            {
                intervalList.Add(SupplementaryInterval.Read(_reader, renamer));
            }

            _stream.Position = returnPosition;

            return intervalList;
        }

        public IIntervalForest<ISupplementaryInterval> GetIntervalForest(IChromosomeRenamer renamer)
        {
            return IntervalArrayFactory.CreateIntervalArray(GetSupplementaryIntervals(renamer), renamer);
        }

        /// <summary>
        /// reads the supplementary annotation from disk
        /// </summary>
        private static SupplementaryAnnotationPosition Read(ExtendedBinaryReader reader, int referencePos)
        {
            var sa = new SupplementaryAnnotationPosition(referencePos);
            SupplementaryAnnotationPosition.Read(reader, sa);

            return sa;
        }

        public bool IsRefMinor(int position)
        {
            return _index.IsRefMinor((uint)position);
        }
    }
}
