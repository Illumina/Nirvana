using System.Collections.Generic;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.Intervals;

namespace VariantAnnotation.Algorithms
{
	public static class IntervalArrayFactory
	{
		public static IIntervalForest<T> CreateIntervalForest<T>(T[] refIntervals, int numRefSeqs)
			where T : IChromosomeInterval
		{
			if (refIntervals == null) return new NullIntervalSearch<T>();

			var intervalLists = new List<Interval<T>>[numRefSeqs];
			for (var i = 0; i < numRefSeqs; i++) intervalLists[i] = new List<Interval<T>>();

			foreach (var transcript in refIntervals)
			{
				intervalLists[transcript.Chromosome.Index].Add(
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
	}
}