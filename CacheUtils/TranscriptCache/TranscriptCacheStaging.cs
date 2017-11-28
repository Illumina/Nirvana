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

            var cacheData = new TranscriptCacheData(header, uniqueData.Genes, uniqueData.Introns, uniqueData.Mirnas,
                uniqueData.PeptideSeqs, transcriptIntervalArrays, regulatoryRegionIntervalArrays);

            return new TranscriptCacheStaging(cacheData);
        }

        private static (IGene[] Genes, IInterval[] Introns, IInterval[] Mirnas, string[] PeptideSeqs) GetUniqueData(
            IEnumerable<IntervalArray<ITranscript>> intervalArrays)
        {
            var intervalComparer = new IntervalComparer();
            var geneComparer     = new GeneComparer();

            var geneSet    = new HashSet<IGene>(geneComparer);
            var intronSet  = new HashSet<IInterval>(intervalComparer);
            var mirnaSet   = new HashSet<IInterval>(intervalComparer);
            var peptideSet = new HashSet<string>();

            foreach (var intervalArray in intervalArrays)
            {
                if (intervalArray == null) continue;

                foreach (var interval in intervalArray.Array)
                {
                    var transcript = interval.Value;
                    geneSet.Add(transcript.Gene);
                    if (transcript.Translation?.PeptideSeq != null) peptideSet.Add(transcript.Translation.PeptideSeq);
                    if (transcript.Introns                 != null) foreach (var intron in transcript.Introns) intronSet.Add(intron);
                    // ReSharper disable once InvertIf
                    if (transcript.MicroRnas               != null) foreach (var mirna in transcript.MicroRnas) mirnaSet.Add(mirna);
                }
            }

            var genes       = geneSet.Count    > 0 ? geneSet.Sort().ToArray()             : null;
            var introns     = intronSet.Count  > 0 ? intronSet.SortInterval().ToArray()   : null;
            var mirnas      = mirnaSet.Count   > 0 ? mirnaSet.SortInterval().ToArray()    : null;
            var peptideSeqs = peptideSet.Count > 0 ? peptideSet.OrderBy(x => x).ToArray() : null;

            return (genes, introns, mirnas, peptideSeqs);
        }
    }
}
