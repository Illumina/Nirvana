using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO.Caches;

namespace Phantom.Utilities
{
    public static class ReadWriteUtilities
    {
        public static (IntervalForest<IGene>, Dictionary<IGene, List<ITranscript>>) GetIntervalAndTranscriptsForeachGene(IntervalArray<ITranscript>[] transcriptIntervalArrays)
        {
            int numChromesomes = transcriptIntervalArrays.Length;
            var geneIntervalArrays = new IntervalArray<IGene>[numChromesomes];
            var geneComparer = new GeneComparer();
            var geneToTranscripts = new Dictionary<IGene, List<ITranscript>>(geneComparer);
            for (int chrIndex = 0; chrIndex < numChromesomes; chrIndex++)
            {
                if (transcriptIntervalArrays[chrIndex] == null)
                {
                    geneIntervalArrays[chrIndex] = new IntervalArray<IGene>(new Interval<IGene>[0]);
                    continue; //TODO: assign an empty IntervalArray to this chr
                }
                var geneList = new List<IGene>(); // keeps the order of genes, as the intervals are already sorted at trasncripts level
                foreach (var transcriptInterval in transcriptIntervalArrays[chrIndex].Array)
                {
                    var transcript = transcriptInterval.Value;
                    var gene = transcript.Gene;
                    if (!geneToTranscripts.ContainsKey(gene))
                    {
                        geneToTranscripts.Add(gene, new List<ITranscript> { transcript });
                        geneList.Add(gene);
                    }
                    else
                    {
                        geneToTranscripts[gene].Append(transcript);
                    }
                }
                geneIntervalArrays[chrIndex] = new IntervalArray<IGene>(geneList.Select(GetGeneInterval).ToArray());
            }
            return (new IntervalForest<IGene>(geneIntervalArrays), geneToTranscripts);
        }

        private static Interval<IGene> GetGeneInterval(IGene gene) => new Interval<IGene>(gene.Start, gene.End, gene);

        public static IntervalArray<ITranscript>[] ReadCache(FileStream fileStream, IDictionary<ushort, IChromosome> refIndexToChromosome)
        {
            IntervalArray<ITranscript>[] transcriptIntervalArrays;
            using (var reader = new TranscriptCacheReader(fileStream))
            {
                transcriptIntervalArrays = reader.Read(refIndexToChromosome).TranscriptIntervalArrays;
            }
            return transcriptIntervalArrays;
        }

        public static void WriteLines(StreamWriter writer, IEnumerable<string> lines) => lines.ToList().ForEach(writer.WriteLine);
    }
}