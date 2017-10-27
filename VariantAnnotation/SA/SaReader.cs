using System;
using System.Collections.Generic;
using System.IO;
using Compression.Algorithms;
using ErrorHandling.Exceptions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;
using VariantAnnotation.Providers;

namespace VariantAnnotation.SA
{
    public class SaReader:ISupplementaryAnnotationReader, IDisposable
    {
        private readonly Stream _stream;
        private readonly ExtendedBinaryReader _reader;

        private readonly MemoryStream _memoryStream;
        private readonly ExtendedBinaryReader _msReader;

        private readonly ISaIndex _index;
        private readonly SaReadBlock _block;
        private long _fileOffset = -1;

        private int _cachedPosition = -1;
        private ISaPosition _cachedSaPosition;

        public IEnumerable<Interval<ISupplementaryInterval>> SmallVariantIntervals { get; }
        public IEnumerable<Interval<ISupplementaryInterval>> SvIntervals { get; }
        public IEnumerable<Interval<ISupplementaryInterval>> AllVariantIntervals { get; }
        public ISupplementaryAnnotationHeader Header { get; }
        public IEnumerable<(int, string)> GlobalMajorAlleleInRefMinors { get; }

        /// <summary>
        /// constructor
        /// </summary>
        public SaReader(Stream stream, Stream idxStream)
        {
            _stream = stream;
            _reader = new ExtendedBinaryReader(stream);

            _block = new SaReadBlock(new Zstandard());

            _memoryStream = new MemoryStream(_block.UncompressedBlock, false);
            _msReader = new ExtendedBinaryReader(_memoryStream);

            _index = SaIndex.Read(idxStream);

            Header = GetHeader(_reader);
            SmallVariantIntervals = GetIntervals();
            SvIntervals = GetIntervals();
            AllVariantIntervals = GetIntervals();
            GlobalMajorAlleleInRefMinors = _index.GlobalMajorAlleleForRefMinor;
        }


        public void Dispose()
        {
            _msReader.Dispose();
            _reader.Dispose();
            _stream.Dispose();
            _memoryStream.Dispose();
        }

        public static ISupplementaryAnnotationHeader GetHeader(ExtendedBinaryReader reader)
        {
            var header = reader.ReadAsciiString();
            var dataVersion = reader.ReadUInt16();
            var schemaVersion = reader.ReadUInt16();
            var genomeAssembly = (GenomeAssembly)reader.ReadByte();

            if (header != SaDataBaseCommon.DataHeader ||
                schemaVersion != SaDataBaseCommon.SchemaVersion)
            {
                throw new UserErrorException($"The header check failed for the supplementary annotation file: ID: exp: {SaDataBaseCommon.DataHeader} obs: {header}, schema version: exp:{SaDataBaseCommon.SchemaVersion} obs: {schemaVersion}");
            }

            var creationTimeTicks = reader.ReadInt64();
            var referenceSequenceName = reader.ReadAsciiString();

            var dataSourceVersions = new HashSet<IDataSourceVersion>();
            var numDataSourceVersions = reader.ReadOptInt32();
            for (var i = 0; i < numDataSourceVersions; i++) dataSourceVersions.Add(DataSourceVersion.Read(reader));

            var saHeader = new SupplementaryAnnotationHeader(referenceSequenceName, creationTimeTicks, dataVersion,
                dataSourceVersions, genomeAssembly);

            return saHeader;
        }

        private IEnumerable<Interval<ISupplementaryInterval>> GetIntervals()
        {
            var numIntervals = _reader.ReadOptInt32();
            var intervals = new List<Interval<ISupplementaryInterval>>(numIntervals);

            for (int i = 0; i < numIntervals; i++)
            {
                var interimInterval = SupplementaryInterval.Read(_reader);
                intervals.Add(new Interval<ISupplementaryInterval>(interimInterval.Start, interimInterval.End, interimInterval));
            }

            return intervals;
        }

        public ISaPosition GetAnnotation(int position)
        {
            // this is used 5400 times in Mother_chr1.genome.vcf.gz
            if (position == _cachedPosition) return _cachedSaPosition;

            var fileOffset = _index.GetOffset(position);
            if (fileOffset < 0) return null;

            if (fileOffset != _fileOffset) SetFileOffset(fileOffset);

            var blockOffset = _block.GetBlockOffset(position);
            if (blockOffset < 0) return null;

            _cachedSaPosition = GetSaPosition(blockOffset);
            _cachedPosition = position;

            return _cachedSaPosition;
        }

        private ISaPosition GetSaPosition(int blockOffset)
        {
            _memoryStream.Position = blockOffset;
            return SaPosition.Read(_msReader);
        }

        private void SetFileOffset(long fileOffset)
        {
            _stream.Position = fileOffset;
            _fileOffset = fileOffset;
            _block.Read(_stream);
        }
    }
}