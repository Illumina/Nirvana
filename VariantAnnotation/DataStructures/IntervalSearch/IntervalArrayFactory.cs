using System.Collections.Generic;
using System.Linq;
using VariantAnnotation.DataStructures.Intervals;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.DataStructures.IntervalSearch
{
    public static class IntervalArrayFactory
    {
        public static IIntervalForest<T> CreateIntervalForest<T>(T[] refIntervals, int numRefSeqs)
            where T : ReferenceAnnotationInterval
        {
            if (refIntervals == null) return new NullIntervalSearch<T>();

            var intervalLists = new List<Interval<T>>[numRefSeqs];
            for (var i = 0; i < numRefSeqs; i++) intervalLists[i] = new List<Interval<T>>();

            foreach (var transcript in refIntervals)
            {
				intervalLists[transcript.ReferenceIndex].Add(
                    new Interval<T>(transcript.Start, transcript.End, transcript));
            }

            // create the interval arrays
            var refIntervalArrays = new IntervalArray<T>[numRefSeqs];
            for (var i = 0; i < numRefSeqs; i++)
            {
                refIntervalArrays[i] = new IntervalArray<T>(intervalLists[i].ToArray());
            }

            return new IntervalForest<T>(refIntervalArrays);
        }

        public static IIntervalForest<IInterimInterval> CreateIntervalArray(
            List<IInterimInterval> intervals, IChromosomeRenamer renamer)
        {
            if (intervals == null || intervals.Count == 0) return new NullIntervalSearch<IInterimInterval>();
            var numRefSeqs = renamer.NumRefSeqs;

            var intervalLists = new List<Interval<IInterimInterval>>[numRefSeqs];
            for (var i = 0; i < numRefSeqs; i++) intervalLists[i] = new List<Interval<IInterimInterval>>();

            foreach (var interval in intervals)
            {
                ushort index = renamer.GetReferenceIndex(interval.ReferenceName);
                if (index == ChromosomeRenamer.UnknownReferenceIndex) continue;

                intervalLists[index].Add(new Interval<IInterimInterval>(interval.Start, interval.End, interval));
            }

            // create the interval arrays
            var refIntervalArrays = new IntervalArray<IInterimInterval>[numRefSeqs];
            for (var i = 0; i < numRefSeqs; i++)
            {
                var sortedIntervals = intervalLists[i].OrderBy(x => x.Begin).ThenBy(x => x.End).ToArray();
                refIntervalArrays[i] = new IntervalArray<IInterimInterval>(sortedIntervals);
            }

            return new IntervalForest<IInterimInterval>(refIntervalArrays);
        }

    }
}
