using System;
using System.Collections.Generic;
using System.IO;
using Compression.FileHandling;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.IO.Caches;

namespace CacheUtils.PredictionCache
{
    public sealed class PredictionCacheWriter : IDisposable
    {
        private readonly BinaryWriter _writer;
        private readonly BlockStream _blockStream;
        private readonly CacheHeader _header;
        private readonly bool _leaveOpen;

        public PredictionCacheWriter(BlockStream blockStream, CacheHeader header, bool leaveOpen = false)
        {
            _blockStream = blockStream;
            _writer      = new BinaryWriter(blockStream);
            _header      = header;
            _leaveOpen   = leaveOpen;
        }

        public void Dispose()
        {
            if (!_leaveOpen) _blockStream.Dispose();
            _writer.Dispose();
        }

        /// <summary>
        /// writes the annotations to the current database file
        /// </summary>
        internal void Write(Prediction.Entry[] lut, Prediction[][] predictionsPerRef)
        {
            _blockStream.WriteHeader(_header.Write);
            WriteLookupTable(_writer, lut);
            _blockStream.Flush();
            WritePredictions(predictionsPerRef);
        }

        private void WritePredictions(IReadOnlyList<Prediction[]> predictionsPerRef)
        {
            // ReSharper disable once UsePatternMatching
            var customHeader = _header.CustomHeader as PredictionCacheCustomHeader;
            if (customHeader == null) throw new InvalidCastException();

            var indexEntries  = customHeader.Entries;
            var blockPosition = new BlockStream.BlockPosition();

            for (var i = 0; i < predictionsPerRef.Count; i++)
            {
	            var refPredictions = predictionsPerRef[i];

				_blockStream.GetBlockPosition(blockPosition);
                indexEntries[i].FileOffset = blockPosition.FileOffset;
                indexEntries[i].Count      = refPredictions?.Length ?? 0;

                if (refPredictions != null)
                {
                    foreach (var prediction in refPredictions) prediction.Write(_writer);
                }

                _blockStream.Flush();
            }
        }

        private static void WriteLookupTable(BinaryWriter writer, IReadOnlyCollection<Prediction.Entry> lut)
        {
            writer.Write(lut.Count);
            foreach (var entry in lut) entry.Write(writer);
        }
    }
}
