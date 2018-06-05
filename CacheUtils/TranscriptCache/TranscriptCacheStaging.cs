using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.MiniCache;
using CacheUtils.TranscriptCache.Comparers;
using Intervals;
using VariantAnnotation.Caches;
using VariantAnnotation.Interface.AnnotatedPositions;
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
                    AddString(peptideSet, transcript.Translation?.PeptideSeq);
                    AddTranscriptRegions(transcriptRegionSet, transcript.TranscriptRegions);
                    AddIntervals(mirnaSet, transcript.MicroRnas);
                }
            }

            var genes             = GetUniqueGenes(geneSet);
            var transcriptRegions = GetUniqueTranscriptRegions(transcriptRegionSet);
            var mirnas            = GetUniqueIntervals(mirnaSet);
            var peptideSeqs       = GetUniqueStrings(peptideSet);

            return (genes, transcriptRegions, mirnas, peptideSeqs);
        }

        private static void AddIntervals(ISet<IInterval> intervalSet, IInterval[] intervals)
        {
            if (intervals == null) return;
            foreach (var interval in intervals) intervalSet.Add(interval);
        }

        private static void AddTranscriptRegions(ISet<ITranscriptRegion> transcriptRegionSet, ITranscriptRegion[] regions)
        {
            if (regions == null) return;
            foreach (var region in regions) transcriptRegionSet.Add(region);
        }

        private static void AddString(ISet<string> stringSet, string s)
        {
            if (string.IsNullOrEmpty(s)) return;
            stringSet.Add(s);
        }

        private static string[] GetUniqueStrings(ICollection<string> peptideSet)
        {
            return peptideSet.Count > 0 ? peptideSet.OrderBy(x => x).ToArray() : null;
        }

        private static IInterval[] GetUniqueIntervals(ICollection<IInterval> mirnaSet)
        {
            return mirnaSet.Count > 0 ? mirnaSet.SortInterval().ToArray() : null;
        }

        private static ITranscriptRegion[] GetUniqueTranscriptRegions(ICollection<ITranscriptRegion> transcriptRegionSet)
        {
            return transcriptRegionSet.Count > 0 ? transcriptRegionSet.SortInterval().ToArray() : null;
        }

        private static IGene[] GetUniqueGenes(ICollection<IGene> geneSet)
        {
            return geneSet.Count > 0 ? geneSet.Sort().ToArray() : null;
        }
    }
}
