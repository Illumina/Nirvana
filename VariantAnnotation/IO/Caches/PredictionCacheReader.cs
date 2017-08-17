using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Compression.Algorithms;
using Compression.FileHandling;
using VariantAnnotation.Caches;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.Caches;

namespace VariantAnnotation.IO.Caches
{
    public sealed class PredictionCacheReader : IDisposable
    {
        private readonly BinaryReader _reader;
        private readonly BlockStream _blockStream;
        private readonly CacheHeader _header;
        private readonly IndexEntry[] _indexEntries;
        private readonly Prediction.Entry[] _lut;
        private readonly string[] _predictionDescriptions;

        public PredictionCacheReader(Stream fs,string[] predictionDescriptions)
        {
            _blockStream  = new BlockStream(new Zstandard(), fs, CompressionMode.Decompress);
            _reader       = new BinaryReader(_blockStream, Encoding.UTF8, true);
            _header       = _blockStream.ReadHeader(CacheHeader.Read, PredictionCacheCustomHeader.Read) as CacheHeader;
            _indexEntries = GetIndexEntries(_header);
            _lut          = ReadLookupTable(_reader);
            _predictionDescriptions = predictionDescriptions;
        }

        private static IndexEntry[] GetIndexEntries(CacheHeader header)
        {
            var customHeader = header.CustomHeader as PredictionCacheCustomHeader;
            return customHeader?.Entries;
        }

        public void Dispose()
        {
            _reader.Dispose();
            _blockStream.Dispose();
        }

        private static Prediction.Entry[] ReadLookupTable(BinaryReader reader)
        {
            var numEntries = reader.ReadInt32();
            var lut = new Prediction.Entry[numEntries];
            for (int i = 0; i < numEntries; i++) lut[i] = Prediction.Entry.Read(reader);
            return lut;
        }

        /// <summary>
        /// parses the database cache file and populates the specified lists and interval trees
        /// </summary>
        public IPredictionCache Read(ushort refIndex)
        {
            var indexEntry = _indexEntries[refIndex];
            var bp         = new BlockStream.BlockPosition { FileOffset = indexEntry.FileOffset };

            _blockStream.SetBlockPosition(bp);

            var predictions = new Prediction[indexEntry.Count];
            for (int i = 0; i < indexEntry.Count; i++) predictions[i] = Prediction.Read(_reader, _lut);

            return new PredictionCache(_header.GenomeAssembly, predictions,_predictionDescriptions);
        }
    }
}