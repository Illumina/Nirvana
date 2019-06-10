using System.Collections.Generic;
using System.Linq;
using Intervals;
using Phantom.Utilities;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace Phantom.CodonInformation
{
    public static class GeneInfoGenerator
    {
        private static readonly IntervalArray<IGene> EmptyIntervalArray = new IntervalArray<IGene>(new Interval<IGene>[0]);

        // ReSharper disable once UnusedTupleComponentInReturnValue
        public static (IntervalForest<IGene>, Dictionary<IGene, List<ITranscript>>) GetGeneIntervalAndTranscripts(IntervalArray<ITranscript>[] transcriptIntervalArrays)
        {
            int numChromesomes     = transcriptIntervalArrays.Length;
            var geneIntervalArrays = new IntervalArray<IGene>[numChromesomes];
            var geneComparer       = new GeneComparer();
            var geneToTranscripts  = new Dictionary<IGene, List<ITranscript>>(geneComparer);

            for (var chrIndex = 0; chrIndex < numChromesomes; chrIndex++)
            {
                if (transcriptIntervalArrays[chrIndex] == null)
                {
                    geneIntervalArrays[chrIndex] = EmptyIntervalArray;
                    continue; // assign an empty IntervalArray to this chr
                }
                var geneList = new List<IGene>(); // keeps the order of genes, as the intervals are already sorted at trasncripts level
                foreach (var transcriptInterval in transcriptIntervalArrays[chrIndex].Array)
                {
                    var transcript = transcriptInterval.Value;
                    if (transcript.Id.IsPredictedTranscript()) continue;

                    var gene = transcript.Gene;
                    if (!geneToTranscripts.ContainsKey(gene))
                    {
                        geneToTranscripts.Add(gene, new List<ITranscript> { transcript });
                        geneList.Add(gene);
                    }
                    else
                    {
                        geneToTranscripts[gene].Add(transcript);
                    }
                }
                geneIntervalArrays[chrIndex] = new IntervalArray<IGene>(geneList.Select(GetGeneInterval).ToArray());
            }
            return (new IntervalForest<IGene>(geneIntervalArrays), geneToTranscripts);
        }

        private static Interval<IGene> GetGeneInterval(IGene gene) => new Interval<IGene>(gene.Start, gene.End, gene);
    }
}