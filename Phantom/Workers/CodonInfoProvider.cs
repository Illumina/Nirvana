using System.Collections.Generic;
using System.Linq;
using Phantom.Interfaces;
using Phantom.Utilities;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;

namespace Phantom.Workers
{
    public sealed class CodonInfoProvider : ICodonInfoProvider
    {
        private IntervalArray<ICodonBlock>[] _codonBlockIntervalArrays;

        private CodonInfoProvider(IntervalArray<ICodonBlock>[] codonBlockIntervalArrays)
        {
            _codonBlockIntervalArrays = codonBlockIntervalArrays;
        }

        public static CodonInfoProvider CreateCodonInfoProvider(IntervalArray<ITranscript>[] transcriptIntervalArrays) => new
            CodonInfoProvider(GetCodonBlockIntervalArrays(transcriptIntervalArrays));

        public List<CodonRange> GetLastCodonRanges(IPosition position)
        {
            //todo; don't really checked the codon info at this time
            var codonRange = new CodonRange(new[] { position.End + 1, position.End + 2 });
            return new List<CodonRange> { codonRange };
        }

        public int GetFunctionBlockRanges(IInterval interval)
        {
            //todo; don't really checked the codon info at this time
            return interval.Start + 2;
        }

        private static IntervalArray<ICodonBlock>[] GetCodonBlockIntervalArrays(
            IntervalArray<ITranscript>[] transcriptIntervalArrays)
        {
            int numChromesomes = transcriptIntervalArrays.Length;
            var codonBlockIntervalArrays = new IntervalArray<ICodonBlock>[numChromesomes];
            for (int chrIndex = 0; chrIndex < numChromesomes; chrIndex++)
            {
                if (transcriptIntervalArrays[chrIndex] == null)
                    continue; //TODO: assign an empty IntervalArray to this chr
                var geneList = new List<IGene>(); // keeps the order of genes, as the intervals are already sorted at trasncripts level
                var geneToCodonBlocks = new Dictionary<IGene, List<ICodonBlock>>(new GeneComparer());
                foreach (var transcriptInterval in transcriptIntervalArrays[chrIndex].Array)
                {
                    var transcript = transcriptInterval.Value;
                    var gene = transcript.Gene;
                    var codonBlocks = ConstructCodonBlocksFromTranscript(transcript);
                    if (!geneToCodonBlocks.ContainsKey(gene))
                    {
                        geneToCodonBlocks.Add(gene, codonBlocks);
                        geneList.Add(gene);
                    }
                    else
                    {
                        geneToCodonBlocks[gene].AddRange(codonBlocks);
                    }
                }
                var allUniqueCodonBlocks = new List<ICodonBlock>();
                geneList.ForEach(x => allUniqueCodonBlocks.AddRange(GetUniqueCodonBlocks(geneToCodonBlocks[x])));
                codonBlockIntervalArrays[chrIndex] = new IntervalArray<ICodonBlock>(allUniqueCodonBlocks.Select(GetCodonBlockInterval).ToArray<Interval<ICodonBlock>>());
            }
            return codonBlockIntervalArrays;
        }

        private static Interval<ICodonBlock> GetCodonBlockInterval(ICodonBlock codonBlock) => new Interval<ICodonBlock>(codonBlock.Start, codonBlock.End, codonBlock);

        private static List<ICodonBlock> GetUniqueCodonBlocks(List<ICodonBlock> codonBlocks)
        {
            return codonBlocks; //todo
        }

        private static List<ICodonBlock> ConstructCodonBlocksFromTranscript(ITranscript transcriptIntervalValue)
        {
            return new List<ICodonBlock>(); //todo
        }
    }

    public struct CodonRange
    {
        public int[] PositionsInCodon;

        public CodonRange(int[] positions)
        {
            PositionsInCodon = positions;
        }
    }
}