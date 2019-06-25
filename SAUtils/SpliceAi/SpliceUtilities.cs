using System.Collections.Generic;
using System.IO;
using System.Linq;
using Intervals;
using IO;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO.Caches;

namespace SAUtils.SpliceAi
{
    public static class SpliceUtilities
    {
        public const int SpliceFlankLength = 15;
        public static Dictionary<ushort, IntervalArray<byte>> GetSpliceIntervals(ISequenceProvider sequenceProvider, Stream cacheStream)
        {
            using (var transcriptCacheReader =
                new TranscriptCacheReader(cacheStream))
            {
                var transcriptData = transcriptCacheReader.Read(sequenceProvider.RefIndexToChromosome);
                var cache = transcriptData.GetCache();

                var spliceIntervals = new Dictionary<ushort, IntervalArray<byte>>(sequenceProvider.RefIndexToChromosome.Count);

                foreach (var chromIndex in sequenceProvider.RefIndexToChromosome.Keys)
                {
                    var spliceInterval = new List<Interval<byte>>(8 * 1024);
                    var overlappingTranscripts =
                        cache.TranscriptIntervalForest.GetAllOverlappingValues(chromIndex, 1, int.MaxValue);

                    if(overlappingTranscripts == null) continue;
                    
                    foreach (var transcript in overlappingTranscripts)
                    {
                        if (transcript.Id.IsPredictedTranscript()) continue;
                        foreach (var transcriptRegion in transcript.TranscriptRegions)
                        {
                            if (transcriptRegion.Type != TranscriptRegionType.Exon) continue;
                            spliceInterval.Add(new Interval<byte>(transcriptRegion.CdnaStart - SpliceFlankLength, transcriptRegion.CdnaStart + SpliceFlankLength, 0));
                            spliceInterval.Add(new Interval<byte>(transcriptRegion.CdnaEnd - SpliceFlankLength, transcriptRegion.CdnaEnd + SpliceFlankLength, 0));
                        }
                    }

                    spliceIntervals[chromIndex] = new IntervalArray<byte>(spliceInterval.OrderBy(x => x.Begin).ThenBy(x => x.End).ToArray());
                }

                return spliceIntervals;
            }
        }

    }
}