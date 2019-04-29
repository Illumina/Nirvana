using System.Collections.Generic;
using System.Linq;
using CacheUtils.Genes.Utilities;
using CacheUtils.MiniCache;
using Genome;
using Intervals;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.TranscriptCache
{
    public static class TranscriptCacheUtilities
    {
        public static List<ITranscript> GetTranscripts(DataBundle bundle, IChromosomeInterval interval)
        {
            ITranscript[] overlappingTranscripts =
                bundle.TranscriptCache.TranscriptIntervalForest.GetAllOverlappingValues(interval.Chromosome.Index,
                    interval.Start, interval.End);
            return overlappingTranscripts?.ToList() ?? new List<ITranscript>();
        }

        public static IntervalArray<T>[] ToIntervalArrays<T>(this IEnumerable<T> items, int numRefSeqs) where T : IChromosomeInterval
        {
            var intervalArrays = new IntervalArray<T>[numRefSeqs];
            var itemsByRef     = items.GetMultiValueDict(x => x.Chromosome.Index);

            foreach (ushort refIndex in itemsByRef.Keys.OrderBy(x => x))
            {
                var unsortedItems = itemsByRef[refIndex];
                var intervals     = unsortedItems.OrderBy(x => x.Start).ThenBy(x => x.End).ToIntervals(unsortedItems.Count);
                intervalArrays[refIndex] = new IntervalArray<T>(intervals);
            }

            return intervalArrays;
        }

        private static Interval<T>[] ToIntervals<T>(this IEnumerable<T> items, int numItems) where T : IChromosomeInterval
        {
            var intervals = new Interval<T>[numItems];
            var i = 0;

            foreach (var item in items)
            {
                intervals[i++] = new Interval<T>(item.Start, item.End, item);
            }

            return intervals;
        }
    }
}
