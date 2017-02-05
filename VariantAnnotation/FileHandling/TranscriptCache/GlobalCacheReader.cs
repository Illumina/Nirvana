using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling.Compression;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;
using ErrorHandling.Exceptions;

namespace VariantAnnotation.FileHandling.TranscriptCache
{
    public sealed class GlobalCacheReader : IDisposable
    {
        #region members

        private readonly ExtendedBinaryReader _reader;
        public FileHeader FileHeader { get; }

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
            if (disposing) _reader.Dispose();
        }

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public GlobalCacheReader(string dbPath)
        {
            var fs          = FileUtilities.GetReadStream(dbPath);
            var blockStream = new BlockStream(new Zstandard(), fs, CompressionMode.Decompress);
            _reader         = new ExtendedBinaryReader(blockStream, Encoding.UTF8);
            FileHeader      = GetHeader(blockStream);
        }

        /// <summary>
        /// constructor (stream)
        /// </summary>
        public GlobalCacheReader(Stream stream)
        {
            var blockStream = new BlockStream(new Zstandard(), stream, CompressionMode.Decompress);
            _reader         = new ExtendedBinaryReader(blockStream, Encoding.UTF8);
            FileHeader      = GetHeader(blockStream);
        }

        private static FileHeader GetHeader(BlockStream blockStream)
        {
            var headerType = FileHeader.GetHeader(0, GenomeAssembly.Unknown, new GlobalCustomHeader(0, 0));
            return (FileHeader)blockStream.ReadHeader(headerType);
        }

        /// <summary>
        /// check if the section guard is in place
        /// </summary>
        private void CheckGuard()
        {
            uint observedGuard = _reader.ReadUInt32();
            if (observedGuard != CacheConstants.GuardInt)
            {
                throw new GeneralException($"Expected a guard integer ({CacheConstants.GuardInt}), but found another value: ({observedGuard})");
            }
        }

        /// <summary>
        /// parses the database cache file and populates the specified lists and interval trees
        /// </summary>
        public GlobalCache Read()
        {
            // read in each of our arrays
            var regulatoryElements = ReadItems(RegulatoryElement.Read);
            var genes              = ReadItems(Gene.Read);
            var introns            = ReadItems(SimpleInterval.Read);
            var mirnas             = ReadItems(SimpleInterval.Read);
            var peptideSeqs        = ReadStringArray();

            // read our transcripts
            var transcripts = ReadTranscripts(genes, introns, mirnas, peptideSeqs);

            return new GlobalCache(FileHeader, transcripts, regulatoryElements, genes, introns, mirnas,
                peptideSeqs);
        }

        private Transcript[] ReadTranscripts(Gene[] genes, SimpleInterval[] introns, SimpleInterval[] mirnas,
            string[] peptideSeqs)
        {
            var numTranscripts = _reader.ReadOptInt32();

            var transcripts = new Transcript[numTranscripts];

            for (int i = 0; i < numTranscripts; i++)
            {
                transcripts[i] = Transcript.Read(_reader, genes, introns, mirnas, peptideSeqs);
            }

            CheckGuard();
            return transcripts;
        }

        /// <summary>
        /// writes all of the items from a array to the output
        /// </summary>
        private T[] ReadItems<T>(Func<ExtendedBinaryReader, T> readObject)
        {
            var numItems = _reader.ReadOptInt32();
            var items = new T[numItems];
            for (int i = 0; i < numItems; i++) items[i] = readObject(_reader);
            CheckGuard();
            return items;
        }

        private string[] ReadStringArray()
        {
            var numStrings = _reader.ReadOptInt32();
            var strings = new string[numStrings];
            for (int i = 0; i < numStrings; i++) strings[i] = _reader.ReadAsciiString();
            CheckGuard();
            return strings;
        }

        public static IFileHeader GetHeader(string cachePath)
        {
            IFileHeader header;
            using (var reader = new GlobalCacheReader(cachePath)) header = reader.FileHeader;
            return header;
        }

        public static GlobalCustomHeader GetCustomHeader(IFileHeader header)
        {
            var customHeader = header.Custom as GlobalCustomHeader;
            if (customHeader == null) throw new InvalidCastException("Could not case header.Custom to GlobalCustomHeader");
            return customHeader;
        }
    }
}
