using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Compression.Algorithms;
using Compression.FileHandling;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.IO;
using VariantAnnotation.IO.Caches;

namespace CacheUtils.IO.Caches
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
        public void Write(ITranscript[] transcripts, IRegulatoryRegion[] regulatoryRegions, IGene[] genes,
            IInterval[] introns, IInterval[] mirnas, string[] peptideSeqs)
        {
            _blockStream.WriteHeader(_header.Write);

            WriteItems(_writer, regulatoryRegions, x => x.Write(_writer));
            WriteItems(_writer, genes,             x => x.Write(_writer));
            WriteItems(_writer, introns,           x => GetInterval(x).Write(_writer));
            WriteItems(_writer, mirnas,            x => GetInterval(x).Write(_writer));
            WriteItems(_writer, peptideSeqs,       x => _writer.WriteOptAscii(x));

            var geneIndices     = CreateIndex(genes);
            var intronIndices   = CreateIndex(introns);
            var microRnaIndices = CreateIndex(mirnas);
            var peptideIndices  = CreateIndex(peptideSeqs);

            WriteItems(_writer, transcripts, x => x.Write(_writer, geneIndices, intronIndices, microRnaIndices, peptideIndices));
        }

        private static Interval GetInterval(IInterval tempInterval) => (Interval)tempInterval;

        internal static void WriteItems<T>(IExtendedBinaryWriter writer, T[] items, Action<T> writeMethod)
        {
            if (items == null)
            {
                writer.WriteOpt(0);
            }
            else
            {
                writer.WriteOpt(items.Length);
                foreach (var item in items) writeMethod(item);
            }

            writer.Write(CacheConstants.GuardInt);
        }

        /// <summary>
        /// creates an index out of a array
        /// </summary>
        internal static Dictionary<T, int> CreateIndex<T>(T[] array)
        {
            var index = new Dictionary<T, int>();
            if (array == null) return index;

            for (int currentIndex = 0; currentIndex < array.Length; currentIndex++)
                index[array[currentIndex]] = currentIndex;

            return index;
        }
    }
}
