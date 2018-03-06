using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.MiniCache;
using CacheUtils.TranscriptCache.Comparers;
using VariantAnnotation.Caches;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.IO.Caches;

namespace CacheUtils.TranscriptCache
{
    public sealed class TranscriptCacheStaging : IStaging
    {
        private readonly TranscriptCacheData _cacheData;

        private TranscriptCacheStaging(TranscriptCacheData cacheData)
        {
            _cacheData = cacheData;
        }

        public void Write(Stream stream)
        {
            using (var writer = new TranscriptCacheWriter(stream, _cacheData.Header)) writer.Write(_cacheData);
        }

        public static TranscriptCacheStaging GetStaging(CacheHeader header,
            IntervalArray<ITranscript>[] transcriptIntervalArrays,
            IntervalArray<IRegulatoryRegion>[] regulatoryRegionIntervalArrays)
        {
            var uniqueData = GetUniqueData(transcriptIntervalArrays);

            var cacheData = new TranscriptCacheData(header, uniqueData.Genes, uniqueData.TranscriptRegions, uniqueData.Mirnas,
                uniqueData.PeptideSeqs, transcriptIntervalArrays, regulatoryRegionIntervalArrays);

            return new TranscriptCacheStaging(cacheData);
        }

        private static (IGene[] Genes, ITranscriptRegion[] TranscriptRegions, IInterval[] Mirnas, string[] PeptideSeqs) GetUniqueData(
            IEnumerable<IntervalArray<ITranscript>> intervalArrays)
        {
            var intervalComparer         = new IntervalComparer();
            var transcriptRegionComparer = new TranscriptRegionComparer();
            var geneComparer             = new GeneComparer();

            var geneSet             = new HashSet<IGene>(geneComparer);
            var transcriptRegionSet = new HashSet<ITranscriptRegion>(transcriptRegionComparer);
            var mirnaSet            = new HashSet<IInterval>(intervalComparer);
            var peptideSet          = new HashSet<string>();

            foreach (var intervalArray in intervalArrays)
            {
                if (intervalArray == null) continue;

                foreach (var interval in intervalArray.Array)
                {
                    var transcript = interval.Value;
                    geneSet.Add(transcript.Gene);
                    if (transcript.Translation?.PeptideSeq != null) peptideSet.Add(transcript.Translation.PeptideSeq);
                    if (transcript.TranscriptRegions       != null) foreach (var region in transcript.TranscriptRegions) transcriptRegionSet.Add(region);
                    // ReSharper disable once InvertIf
                    if (transcript.MicroRnas               != null) foreach (var mirna in transcript.MicroRnas) mirnaSet.Add(mirna);
                }
            }

            var genes             = geneSet.Count             > 0 ? geneSet.Sort().ToArray()                     : null;
            var transcriptRegions = transcriptRegionSet.Count > 0 ? transcriptRegionSet.SortInterval().ToArray() : null;
            var mirnas            = mirnaSet.Count            > 0 ? mirnaSet.SortInterval().ToArray()            : null;
            var peptideSeqs       = peptideSet.Count          > 0 ? peptideSet.OrderBy(x => x).ToArray()         : null;

            return (genes, transcriptRegions, mirnas, peptideSeqs);
        }
    }
}
