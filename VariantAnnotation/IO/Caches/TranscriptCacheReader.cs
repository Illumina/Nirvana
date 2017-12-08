using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Compression.Algorithms;
using Compression.FileHandling;
using ErrorHandling.Exceptions;
using VariantAnnotation.Caches;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Providers;

namespace VariantAnnotation.IO.Caches
{
    public sealed class TranscriptCacheReader : IDisposable
    {
        private readonly ExtendedBinaryReader _reader;
        private readonly ushort _numRefSequences;
        private readonly CacheHeader _header;

        public TranscriptCacheReader(Stream stream, GenomeAssembly genomeAssembly, ushort numRefSequences)
        {
            var blockStream  = new BlockStream(new Zstandard(), stream, CompressionMode.Decompress);
            _reader          = new ExtendedBinaryReader(blockStream, Encoding.UTF8);
            _header          = blockStream.ReadHeader(CacheHeader.Read, TranscriptCacheCustomHeader.Read) as CacheHeader;
            _numRefSequences = numRefSequences;

	        if (genomeAssembly != _header?.GenomeAssembly)
	        {
	            throw new UserErrorException("Found more than one genome assembly represented in the selected data sources.");
            }

            if (_header.SchemaVersion != CacheConstants.SchemaVersion)
            {
                throw new UserErrorException($"The selected cache file has a different version (Schema: {_header.SchemaVersion}, Data: {_header.DataVersion}) than expected (Schema: {CacheConstants.SchemaVersion}, Data: {CacheConstants.DataVersion})");
            }
        }

        public void Dispose() => _reader.Dispose();

        /// <summary>
        /// parses the database cache file and populates the specified lists and interval trees
        /// </summary>
        public TranscriptCache Read(IDictionary<ushort, IChromosome> refIndexToChromosome)
        {
            var regulatoryElements = ReadItems(_reader, () => RegulatoryRegion.Read(_reader, refIndexToChromosome));
            var genes              = ReadItems(_reader, () => Gene.Read(_reader, refIndexToChromosome));
            var introns            = ReadItems(_reader, () => Interval.Read(_reader));
            var mirnas             = ReadItems(_reader, () => Interval.Read(_reader));
            var peptideSeqs        = ReadItems(_reader, () => _reader.ReadAsciiString());
            var transcripts        = ReadItems(_reader, () => Transcript.Read(_reader, refIndexToChromosome, genes, introns, mirnas, peptideSeqs));

            var genomeAssembly = _header.GenomeAssembly;
            var dataSourceVersions = GetDataSourceVersions(_header);

            return new TranscriptCache(dataSourceVersions, genomeAssembly, transcripts, regulatoryElements, _numRefSequences);
        }

        internal static T[] ReadItems<T>(IExtendedBinaryReader reader, Func<T> readMethod)
        {
            var numItems = reader.ReadOptInt32();
            var items = new T[numItems];
            for (int i = 0; i < numItems; i++) items[i] = readMethod();
            CheckGuard(reader);
            return items;
        }

        /// <summary>
        /// check if the section guard is in place
        /// </summary>
        internal static void CheckGuard(IExtendedBinaryReader reader)
        {
            uint observedGuard = reader.ReadUInt32();
            if (observedGuard != CacheConstants.GuardInt)
            {
                throw new InvalidDataException($"Expected a guard integer ({CacheConstants.GuardInt}), but found another value: ({observedGuard})");
            }
        }

        private static IEnumerable<IDataSourceVersion> GetDataSourceVersions(CacheHeader header)
        {
            var vepVersion = ((TranscriptCacheCustomHeader)header.CustomHeader).VepVersion;
            var dataSourceVersion = new DataSourceVersion("VEP", vepVersion.ToString(), header.CreationTimeTicks, header.TranscriptSource.ToString());

            return new List<IDataSourceVersion> { dataSourceVersion };
        }
    }
}