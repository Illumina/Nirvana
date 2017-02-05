using System.Collections.Generic;
using System.Linq;
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

			//Array.Sort(refIntervals);
            var intervalLists = new List<IntervalArray<T>.Interval>[numRefSeqs];
            for (var i = 0; i < numRefSeqs; i++) intervalLists[i] = new List<IntervalArray<T>.Interval>();

            foreach (var transcript in refIntervals)
            {
				//debugging to see if both ensembl and refseq transcripts come up

				//var Transcript = transcript as Transcript;
				//if (Transcript?.TranscriptSource == TranscriptDataSource.RefSeq)
				//	Console.WriteLine("found refseq transcript");

				intervalLists[transcript.ReferenceIndex].Add(
                    new IntervalArray<T>.Interval(transcript.Start, transcript.End, transcript));
            }

            // create the interval arrays
            var refIntervalArrays = new IntervalArray<T>[numRefSeqs];
            for (var i = 0; i < numRefSeqs; i++)
            {
                refIntervalArrays[i] = new IntervalArray<T>(intervalLists[i].ToArray());
            }

            return new IntervalForest<T>(refIntervalArrays);
        }

        public static IIntervalForest<ISupplementaryInterval> CreateIntervalArray(
            IEnumerable<ISupplementaryInterval> intervals, IChromosomeRenamer renamer)
        {
            if (intervals == null) return new NullIntervalSearch<ISupplementaryInterval>();

            var numRefSeqs = renamer.NumRefSeqs;

            var intervalLists = new List<IntervalArray<ISupplementaryInterval>.Interval>[numRefSeqs];
            for (var i = 0; i < numRefSeqs; i++) intervalLists[i] = new List<IntervalArray<ISupplementaryInterval>.Interval>();

            foreach (var interval in intervals)
            {
                ushort index = renamer.GetReferenceIndex(interval.ReferenceName);
                if (index == ChromosomeRenamer.UnknownReferenceIndex) continue;

                intervalLists[index].Add(new IntervalArray<ISupplementaryInterval>.Interval(interval.Start, interval.End, interval));
            }

            // create the interval arrays
            var refIntervalArrays = new IntervalArray<ISupplementaryInterval>[numRefSeqs];
            for (var i = 0; i < numRefSeqs; i++)
            {
                var sortedIntervals = intervalLists[i].OrderBy(x => x.Begin).ThenBy(x => x.End).ToArray();
                refIntervalArrays[i] = new IntervalArray<ISupplementaryInterval>(sortedIntervals);
            }

            return new IntervalForest<ISupplementaryInterval>(refIntervalArrays);
        }

        public static IIntervalForest<ICustomInterval> CreateIntervalArray(IEnumerable<ICustomInterval> intervals, IChromosomeRenamer renamer)
        {
            if (intervals == null) return new NullIntervalSearch<ICustomInterval>();

            var numRefSeqs = renamer.NumRefSeqs;

            var intervalLists = new List<IntervalArray<ICustomInterval>.Interval>[numRefSeqs];
            for (var i = 0; i < numRefSeqs; i++) intervalLists[i] = new List<IntervalArray<ICustomInterval>.Interval>();

            foreach (var interval in intervals)
            {
                ushort index = renamer.GetReferenceIndex(interval.ReferenceName);
                if (index == ChromosomeRenamer.UnknownReferenceIndex) continue;

                intervalLists[index].Add(new IntervalArray<ICustomInterval>.Interval(interval.Start, interval.End, interval));
            }

            // create the interval arrays
            var refIntervalArrays = new IntervalArray<ICustomInterval>[numRefSeqs];
            for (var i = 0; i < numRefSeqs; i++)
            {
                var sortedIntervals = intervalLists[i].OrderBy(x => x.Begin).ThenBy(x => x.End).ToArray();
                refIntervalArrays[i] = new IntervalArray<ICustomInterval>(sortedIntervals);
            }

            return new IntervalForest<ICustomInterval>(refIntervalArrays);
        }
    }
}
