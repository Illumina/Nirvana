using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using CacheUtils.TranscriptCache.Comparers;
using Compression.Algorithms;
using Compression.FileHandling;
using VariantAnnotation.Caches;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.IO;
using VariantAnnotation.IO.Caches;


namespace CacheUtils.TranscriptCache
{
    public sealed class TranscriptCacheWriter : IDisposable
    {
        private readonly BlockStream _blockStream;
        private readonly ExtendedBinaryWriter _writer;
        private readonly CacheHeader _header;
        private readonly bool _leaveOpen;

        public TranscriptCacheWriter(Stream stream, CacheHeader header, bool leaveOpen = false)
        {
            _blockStream = new BlockStream(new Zstandard(), stream, CompressionMode.Compress);
            _writer      = new ExtendedBinaryWriter(_blockStream, Encoding.UTF8, leaveOpen);
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
        public void Write(TranscriptCacheData cacheData)
        {
            _blockStream.WriteHeader(_header.Write);

            WriteItems(_writer, cacheData.Genes,             x => x.Write(_writer));
            WriteItems(_writer, cacheData.TranscriptRegions, x => x.Write(_writer));
            WriteItems(_writer, cacheData.Mirnas,            x => ((ISerializable)x).Write(_writer));
            WriteItems(_writer, cacheData.PeptideSeqs,       x => _writer.WriteOptAscii(x));

            var geneComparer             = new GeneComparer();
            var transcriptRegionComparer = new TranscriptRegionComparer();
            var intervalComparer         = new IntervalComparer();

            var geneIndices             = CreateIndex(cacheData.Genes, geneComparer);
            var transcriptRegionIndices = CreateIndex(cacheData.TranscriptRegions, transcriptRegionComparer);
            var microRnaIndices         = CreateIndex(cacheData.Mirnas, intervalComparer);
            var peptideIndices          = CreateIndex(cacheData.PeptideSeqs, EqualityComparer<string>.Default);

            WriteIntervals(_writer, cacheData.RegulatoryRegionIntervalArrays, x => x.Write(_writer));
            WriteIntervals(_writer, cacheData.TranscriptIntervalArrays,       x => x.Write(_writer, geneIndices, transcriptRegionIndices, microRnaIndices, peptideIndices));
        }

        private static void WriteIntervals<T>(IExtendedBinaryWriter writer, IReadOnlyCollection<IntervalArray<T>> intervalArrays,
            Action<T> writeMethod)
        {
            writer.WriteOpt(intervalArrays.Count);

            foreach (var intervalArray in intervalArrays)
            {
                if (intervalArray == null)
                {
                    writer.WriteOpt(0);
                    continue;
                }

                writer.WriteOpt(intervalArray.Array.Length);
                foreach (var interval in intervalArray.Array) writeMethod(interval.Value);
            }

            writer.Write(CacheConstants.GuardInt);
        }

        internal static void WriteItems<T>(IExtendedBinaryWriter writer, IReadOnlyCollection<T> items, Action<T> writeMethod)
        {
            if (items == null)
            {
                writer.WriteOpt(0);
            }
            else
            {
                writer.WriteOpt(items.Count);
                foreach (var item in items) writeMethod(item);
            }

            writer.Write(CacheConstants.GuardInt);
        }

        /// <summary>
        /// creates an index out of a array
        /// </summary>
        internal static Dictionary<T, int> CreateIndex<T>(IReadOnlyList<T> array, IEqualityComparer<T> comparer)
        {
            var index = new Dictionary<T, int>(comparer);
            if (array == null) return index;

            for (var currentIndex = 0; currentIndex < array.Count; currentIndex++)
                index[array[currentIndex]] = currentIndex;

            return index;
        }
    }
}
