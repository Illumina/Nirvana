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
        private readonly string[] _predictionDescriptions;
        private readonly IndexEntry[] _indexEntries;
        public readonly PredictionHeader Header;

        public PredictionCacheReader(Stream fs, string[] predictionDescriptions)
        {
            _blockStream            = new BlockStream(new Zstandard(), fs, CompressionMode.Decompress);
            _reader                 = new BinaryReader(_blockStream, Encoding.UTF8, true);
            _predictionDescriptions = predictionDescriptions;

            Header        = GetHeader();
            _indexEntries = GetIndexEntries(Header.Header);
        }

        private PredictionHeader GetHeader()
        {
            var header = _blockStream.ReadHeader(CacheHeader.Read, PredictionCacheCustomHeader.Read) as CacheHeader;
            var lut    = ReadLookupTable(_reader);
            return new PredictionHeader(header, lut);
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
            var predictions = GetPredictions(refIndex);
            return new PredictionCache(Header.Header.GenomeAssembly, predictions, _predictionDescriptions);
        }

        public Prediction[] GetPredictions(ushort refIndex)
        {
            var indexEntry = _indexEntries[refIndex];
            var bp = new BlockStream.BlockPosition { FileOffset = indexEntry.FileOffset };

            _blockStream.SetBlockPosition(bp);

            var predictions = new Prediction[indexEntry.Count];
            for (int i = 0; i < indexEntry.Count; i++) predictions[i] = Prediction.Read(_reader, Header.Lut);

            return predictions;
        }

        public static readonly string[] SiftDescriptions =
        {
            "tolerated", "deleterious", "tolerated - low confidence",
            "deleterious - low confidence"
        };

        public static readonly string[] PolyphenDescriptions =
        {
            "probably damaging", "possibly damaging", "benign", "unknown"
        };

        public sealed class PredictionHeader
        {
            public readonly CacheHeader Header;
            public readonly Prediction.Entry[] Lut;

            public PredictionHeader(CacheHeader header, Prediction.Entry[] lut)
            {
                Header = header;
                Lut    = lut;
            }
        }
    }
}