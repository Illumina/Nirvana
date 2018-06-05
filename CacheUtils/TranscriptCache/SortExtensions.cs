using System.Collections.Generic;
using System.Linq;
using Genome;
using Intervals;

namespace CacheUtils.TranscriptCache
{
    public static class SortExtensions
    {
        public static IOrderedEnumerable<T> Sort<T>(this IEnumerable<T> elements) where T : IChromosomeInterval =>
            elements.OrderBy(x => x.Chromosome.Index).ThenBy(x => x.Start).ThenBy(x => x.End);

        public static IOrderedEnumerable<T> SortInterval<T>(this IEnumerable<T> elements) where T : IInterval =>
            elements.OrderBy(x => x.Start).ThenBy(x => x.End);
    }
}
