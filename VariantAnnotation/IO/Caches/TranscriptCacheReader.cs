using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Compression.Algorithms;
using Compression.FileHandling;
using Genome;
using Intervals;
using IO;
using VariantAnnotation.Caches;
using VariantAnnotation.Caches.DataStructures;

namespace VariantAnnotation.IO.Caches
{
    public sealed class TranscriptCacheReader : IDisposable
    {
        private readonly BufferedBinaryReader _reader;
        public readonly CacheHeader Header;

        public TranscriptCacheReader(Stream stream)
        {
            Header          = CacheHeader.Read(stream);
            var blockStream = new BlockStream(new Zstandard(), stream, CompressionMode.Decompress);
            _reader         = new BufferedBinaryReader(blockStream);
        }

        public void Dispose() => _reader.Dispose();

        /// <summary>
        /// parses the database cache file and populates the specified lists and interval trees
        /// </summary>
        public TranscriptCacheData Read(IDictionary<ushort, IChromosome> refIndexToChromosome)
        {
            var genes             = ReadItems(_reader,     () => Gene.Read(_reader, refIndexToChromosome));
            var transcriptRegions = ReadItems(_reader,     () => TranscriptRegion.Read(_reader));
            var mirnas            = ReadItems(_reader,     () => IntervalExtensions.Read(_reader));
            var peptideSeqs       = ReadItems(_reader,     () => _reader.ReadAsciiString());
            var regulatoryRegions = ReadIntervals(_reader, () => RegulatoryRegion.Read(_reader, refIndexToChromosome));
            var transcripts       = ReadIntervals(_reader, () => Transcript.Read(_reader, refIndexToChromosome, genes, transcriptRegions, mirnas, peptideSeqs));

            return new TranscriptCacheData(Header, genes, transcriptRegions, mirnas, peptideSeqs, transcripts, regulatoryRegions);
        }

        private static IntervalArray<T>[] ReadIntervals<T>(IBufferedBinaryReader reader, Func<T> readMethod) where T : IInterval
        {
            var numRefSeqs     = reader.ReadOptInt32();
            var intervalArrays = new IntervalArray<T>[numRefSeqs];

            for (int refSeqIndex = 0; refSeqIndex < numRefSeqs; refSeqIndex++)
            {
                var numItems  = reader.ReadOptInt32();
                if (numItems == 0) continue;

                var intervals = new Interval<T>[numItems];

                for (int i = 0; i < numItems; i++)
                {
                    var item = readMethod();
                    intervals[i] = new Interval<T>(item.Start, item.End, item);
                }

                intervalArrays[refSeqIndex] = new IntervalArray<T>(intervals);
            }

            CheckGuard(reader);
            return intervalArrays;
        }

        internal static T[] ReadItems<T>(IBufferedBinaryReader reader, Func<T> readMethod)
        {
            var numItems = reader.ReadOptInt32();
            var items    = new T[numItems];
            for (int i = 0; i < numItems; i++) items[i] = readMethod();
            CheckGuard(reader);
            return items;
        }

        /// <summary>
        /// check if the section guard is in place
        /// </summary>
        internal static void CheckGuard(IBufferedBinaryReader reader)
        {
            uint observedGuard = reader.ReadUInt32();
            if (observedGuard != CacheConstants.GuardInt)
            {
                throw new InvalidDataException($"Expected a guard integer ({CacheConstants.GuardInt}), but found another value: ({observedGuard})");
            }
        }
    }
}