using System;
using System.Collections.Generic;
using System.Linq;
using VariantAnnotation.DataStructures.IntervalSearch;
using VariantAnnotation.Interface;

namespace TestIntervalSearch
{
    public static class Factories
    {
        public const int NumRefSeqs = 25;

        public static IIntervalForest<int> CreateIntervalArray(List<Tuple<ushort, int, int, int>> items)
        {
            var intervalLists = new List<IntervalArray<int>.Interval>[NumRefSeqs];
            for (int i = 0; i < NumRefSeqs; i++) intervalLists[i] = new List<IntervalArray<int>.Interval>();

            foreach (var item in items)
            {
                intervalLists[item.Item1].Add(new IntervalArray<int>.Interval(item.Item2, item.Item3, item.Item4));
            }

            // create the interval arrays
            var refIntervalArrays = new IntervalArray<int>[NumRefSeqs];
            for (int i = 0; i < NumRefSeqs; i++)
            {
                var sortedIntervals = intervalLists[i].OrderBy(x => x.Begin).ThenBy(x => x.End).ToArray();
                refIntervalArrays[i] = new IntervalArray<int>(sortedIntervals);
            }

            return new IntervalForest<int>(refIntervalArrays);
        }
    }
}
