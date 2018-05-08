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
        private readonly PredictionHeader _header;
        private readonly bool _leaveOpen;

        public PredictionCacheWriter(BlockStream blockStream, PredictionHeader header, bool leaveOpen = false)
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

        internal void Write(Prediction.Entry[] lut, Prediction[][] predictionsPerRef)
        {
            _blockStream.WriteHeader(_header.Write);
            WriteLookupTable(_writer, lut);
            _blockStream.Flush();
            WritePredictions(predictionsPerRef);
        }

        private void WritePredictions(IReadOnlyList<Prediction[]> predictionsPerRef)
        {
            var indexEntries = _header.Custom.Entries;

            for (var i = 0; i < predictionsPerRef.Count; i++)
            {
	            var refPredictions = predictionsPerRef[i];

				var position = _blockStream.GetBlockPosition();
                indexEntries[i].FileOffset = position.FileOffset;
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
