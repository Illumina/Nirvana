using System.Collections.Generic;
using System.Linq;
using Intervals;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.Caches.Utilities
{
    public static class GeneForestGenerator
    {
        private static readonly IntervalArray<IGene> EmptyIntervalArray = new IntervalArray<IGene>(new Interval<IGene>[0]);

        public static IntervalForest<IGene> GetGeneForest(IntervalArray<ITranscript>[] transcriptIntervalArrays)
        {
            int numChromosomes     = transcriptIntervalArrays.Length;
            var geneIntervalArrays = new IntervalArray<IGene>[numChromosomes];
            var geneComparer       = new GeneComparer();

            for (var chrIndex = 0; chrIndex < numChromosomes; chrIndex++)
            {
                if (transcriptIntervalArrays[chrIndex] == null)
                {
                    geneIntervalArrays[chrIndex] = EmptyIntervalArray;
                    continue; // assign an empty IntervalArray to this chr
                }
                var geneList = new List<IGene>(); // keeps the order of genes, as the intervals are already sorted at trasncripts level
                var geneSet = new HashSet<IGene>(geneComparer);
                foreach (var transcriptInterval in transcriptIntervalArrays[chrIndex].Array)
                {
                    var transcript = transcriptInterval.Value;
                    if (transcript.Id.IsPredictedTranscript()) continue;

                    var gene = transcript.Gene;
                    if (geneSet.Contains(gene)) continue;

                    geneSet.Add(gene);
                    geneList.Add(gene);
                }
                geneIntervalArrays[chrIndex] = new IntervalArray<IGene>(geneList.Select(GetGeneInterval).ToArray());
            }
            return new IntervalForest<IGene>(geneIntervalArrays);
        }

        private static Interval<IGene> GetGeneInterval(IGene gene) => new Interval<IGene>(gene.Start, gene.End, gene);
    }
}