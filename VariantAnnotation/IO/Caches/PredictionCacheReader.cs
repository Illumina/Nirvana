﻿using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Compression.Algorithms;
using Compression.FileHandling;
using IO;
using VariantAnnotation.Caches;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.Caches;

namespace VariantAnnotation.IO.Caches
{
    public sealed class PredictionCacheReader : IDisposable
    {
        private readonly ExtendedBinaryReader _reader;
        private readonly BlockStream _blockStream;
        private readonly string[] _predictionDescriptions;
        private readonly IndexEntry[] _indexEntries;
        private readonly int _numRefSeqs;
        public readonly PredictionHeader Header;

        public PredictionCacheReader(Stream stream, string[] predictionDescriptions)
        {
            _blockStream = new BlockStream(new Zstandard(), stream, CompressionMode.Decompress);
            Header       = PredictionHeader.Read(stream, _blockStream);

            _reader = new ExtendedBinaryReader(_blockStream, Encoding.Default, true);
            _predictionDescriptions = predictionDescriptions;

            _indexEntries = Header.Custom.Entries;
            _numRefSeqs = _indexEntries.Length;
        }

        public void Dispose()
        {
            _reader.Dispose();
            _blockStream.Dispose();
        }

        /// <summary>
        /// parses the database cache file and populates the specified lists and interval trees
        /// </summary>
        public IPredictionCache Read(ushort refIndex)
        {
            if (refIndex >= _numRefSeqs) return null;
            var predictions = GetPredictions(refIndex);
            return new PredictionCache(Header.Assembly, predictions, _predictionDescriptions);
        }

        public Prediction[] GetPredictions(ushort refIndex)
        {
            var indexEntry = _indexEntries[refIndex];

            _blockStream.SetBlockPosition(indexEntry.FileOffset);

            var predictions = new Prediction[indexEntry.Count];
            for (var i = 0; i < indexEntry.Count; i++) predictions[i] = Prediction.Read(_reader, Header.LookupTable);

            return predictions;
        }

        public static readonly string[] SiftDescriptions = new string[]{"tolerated",
            "deleterious", "tolerated - low confidence", "deleterious - low confidence"};

        public static readonly string[] PolyphenDescriptions =
            new string[]{"probably damaging", "possibly damaging", "benign", "unknown"};
    }
}