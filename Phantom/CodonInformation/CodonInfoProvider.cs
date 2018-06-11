using System;
using System.Collections.Generic;
using System.Linq;
using Genome;
using Intervals;
using Phantom.Utilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using Phantom.Graph;

namespace Phantom.CodonInformation
{
    public sealed class CodonInfoProvider : ICodonInfoProvider
    {
        private readonly IntervalForest<ICodingBlock> _commonIntervalForest;
        // ReSharper disable once NotAccessedField.Local
        private readonly Graph<ICodingBlock>[] _codingBlockGraphs;

        private CodonInfoProvider(IntervalForest<ICodingBlock> commonIntervalForest, Graph<ICodingBlock>[] codingBlockGraphs)
        {
            _commonIntervalForest = commonIntervalForest;
            _codingBlockGraphs = codingBlockGraphs;
        }

        public static CodonInfoProvider CreateCodonInfoProvider(IntervalArray<ITranscript>[] transcriptIntervalArrays)
        {
            var numChromosomes = transcriptIntervalArrays.Length;
            var commonIntervalArrays = new IntervalArray<ICodingBlock>[numChromosomes];
            var codingBlockGraphs = new Graph<ICodingBlock>[numChromosomes];

            for (int chrIndex = 0; chrIndex < numChromosomes; chrIndex++)
            {
                codingBlockGraphs[chrIndex] = new Graph<ICodingBlock>(true);
                var transcriptIntervalArray = transcriptIntervalArrays[chrIndex];
                if (transcriptIntervalArray == null) continue;
                var geneCdsIntervals = GetPhasedCdsIntervals(transcriptIntervalArray);
                var intervalsWithPhase = new List<Interval<ICodingBlock>>();

                foreach (var (gene, transcriptIntervals) in geneCdsIntervals)
                {
                    var transcriptToCodingBlocks =
                        GetTranscriptToCodingBlocks(transcriptIntervals, gene.OnReverseStrand);
                    codingBlockGraphs[chrIndex].MergeGraph(GetCodingGraph(transcriptToCodingBlocks));
                    intervalsWithPhase.AddRange(GetIntervalsWithPhase(transcriptToCodingBlocks));
                }
                commonIntervalArrays[chrIndex] = new IntervalArray<ICodingBlock>(intervalsWithPhase.OrderBy(x => x.Begin).ToArray());
            }
            return new CodonInfoProvider(new IntervalForest<ICodingBlock>(commonIntervalArrays), codingBlockGraphs);
        }

        private static ICollection<Interval<ICodingBlock>> GetIntervalsWithPhase(CodingBlock[][] transcriptToCodingBlocks)
        {
            var uniqCodingBlocks = new HashSet<Interval<ICodingBlock>>();
            foreach (var codingBlocks in transcriptToCodingBlocks)
                foreach (var codingBlock in codingBlocks)
                {
                    var codingBlockInterval = new Interval<ICodingBlock>(codingBlock.Start, codingBlock.End, codingBlock);
                    uniqCodingBlocks.Add(codingBlockInterval);
                }
            return uniqCodingBlocks;
        }

        public int GetLongestFunctionBlockDistance(IChromosomeInterval chrInterval)
        {
            var functionBlockRanges = GetFunctionBlockDistances(chrInterval);
            if (functionBlockRanges == null) return -1;
            int longestRange = functionBlockRanges[0];
            for (int i = 1; i < functionBlockRanges.Length; i++)
            {
                if (functionBlockRanges[i] > longestRange) longestRange = functionBlockRanges[i];
            }
            return longestRange;
        }

        public int[] GetFunctionBlockDistances(IChromosomeInterval chrInterval)
        {
            var overlappingCodingBlocks = GetOverlappingCodingBlocks(chrInterval);
            if (overlappingCodingBlocks == null) return null;
            var functionBlockRanges = new int[overlappingCodingBlocks.Length];
            for (int i = 0; i < overlappingCodingBlocks.Length; i++)
            {
                var overlappingCodingBlock = overlappingCodingBlocks[i];
                functionBlockRanges[i] = GetFunctionBlockRange(chrInterval, overlappingCodingBlock);
            }
            return functionBlockRanges;
        }

        private ICodingBlock[] GetOverlappingCodingBlocks(IChromosomeInterval chrInterval) =>
            _commonIntervalForest.GetAllOverlappingValues(chrInterval.Chromosome.Index, chrInterval.Start,
                chrInterval.End);

        private int GetFunctionBlockRange(IInterval interval, ICodingBlock overlappingCodingBlock)
        {
            //todo: only check codon boundary in the same exon for now
            return GetCodonRange(interval.Start, overlappingCodingBlock);
        }

        internal static int GetCodonRange(int position, ICodingBlock codingBlock)
        {
            int currentPhase = (position - codingBlock.Start + codingBlock.StartPhase) % 3;
            int range = position + 2 - currentPhase;
            return Math.Min(range, codingBlock.End); //don't cross the block boundary for now
        }

        private static Dictionary<IGene, List<PhasedIntervalArray>> GetPhasedCdsIntervals(
            IntervalArray<ITranscript> transcriptIntervalArray)
        {
            var geneToCodingIntervals = new Dictionary<IGene, List<PhasedIntervalArray>>(new GeneComparer());
            foreach (var transcriptInterval in transcriptIntervalArray.Array)
            {
                var transcript = transcriptInterval.Value;
                var gene = transcript.Gene;
                byte startPhase = transcript.StartExonPhase;
                var codingIntervals = ConstructCdsIntervalsFromTranscript(transcript);
                if (codingIntervals == null) continue;
                var phasedIntervals = new PhasedIntervalArray(startPhase, codingIntervals);
                if (geneToCodingIntervals.TryGetValue(gene, out var transcriptIntervals))
                {
                    transcriptIntervals.Add(phasedIntervals);
                }
                else
                {
                    geneToCodingIntervals.Add(gene, new List<PhasedIntervalArray> { phasedIntervals });
                }
            }
            return geneToCodingIntervals;
        }

        private static Graph<ICodingBlock> GetCodingGraph(CodingBlock[][] transcriptToCodingBlocks)
        {
            var codingGraph = new Graph<ICodingBlock>(true);
            foreach (var codingBlocks in transcriptToCodingBlocks)
            {
                codingGraph.TryAddVertex(codingBlocks[0]);
                for (var i = 1; i < codingBlocks.Length; i++)
                {
                    codingGraph.AddEdge(codingBlocks[i - 1], codingBlocks[i]);
                }
            }
            return codingGraph;
        }

        internal static CodingBlock[][] GetTranscriptToCodingBlocks(List<PhasedIntervalArray> transcriptIntervals, bool onReverseStrand)
        {
            var transcriptToCommonIntervals = IntervalPartitioner.GetCommonIntervals(new TranscriptIntervalsInGene(transcriptIntervals.Select(x => x.IntervalArray).ToArray()));
            var startPhases = transcriptIntervals.Select(x => x.StartPhase).ToArray();
            var codingBlockArrays = new CodingBlock[transcriptToCommonIntervals.Length][];
            if (onReverseStrand)
            {
                startPhases = GetStartPhaseForReverseTranscripts(transcriptToCommonIntervals, startPhases);
            }

            for (int transcriptId = 0; transcriptId < transcriptToCommonIntervals.Length; transcriptId++)
            {
                byte currentPhase = startPhases[transcriptId];
                var commonIntervals = transcriptToCommonIntervals[transcriptId];
                codingBlockArrays[transcriptId] = new CodingBlock[commonIntervals.Count];
                for (int intervalId = 0; intervalId < commonIntervals.Count; intervalId++)
                {
                    var interval = commonIntervals[intervalId];
                    var codingBlock = new CodingBlock(interval.Start, interval.End,
                        currentPhase);
                    codingBlockArrays[transcriptId][intervalId] = codingBlock;
                    currentPhase = UpdateStartPhase(interval, currentPhase);
                }
            }
            return codingBlockArrays;
        }

        private static byte[] GetStartPhaseForReverseTranscripts(List<IInterval>[] transcriptToIntervals, byte[] startPhases)
        {
            var startPhasesFromEndOfReverseTranscripts = new byte[transcriptToIntervals.Length];
            for (int i = 0; i < transcriptToIntervals.Length; i++)
            {
                int totalLength = GetTotalIntervalLength(transcriptToIntervals[i]);
                startPhasesFromEndOfReverseTranscripts[i] = (byte)(2 - (startPhases[i] + totalLength % 3 + 2) % 3);
            }
            return startPhasesFromEndOfReverseTranscripts;
        }

        private static int GetTotalIntervalLength(List<IInterval> transcriptToInterval)
        {
            int totalLength = 0;
            foreach (var interval in transcriptToInterval)
            {
                totalLength += interval.End - interval.Start + 1;
            }
            return totalLength;
        }

        private static byte UpdateStartPhase(IInterval interval, byte currentStartPhase) => (byte)((interval.End - interval.Start + 1 + currentStartPhase) % 3);


        private static IInterval[] ConstructCdsIntervalsFromTranscript(ITranscript transcript)
        {
            var cdsIntervals = new List<IInterval>();
            if (transcript.Translation == null) return null;

            ICodingRegion codingRegion = transcript.Translation.CodingRegion;
            List<ITranscriptRegion> exonRegions = transcript.TranscriptRegions.Where(x => x.Type == TranscriptRegionType.Exon).ToList();

            if (codingRegion == null) return null; // no coding region

            foreach (var exonRegion in exonRegions) // assume the exon regions are ordered
            {
                var exonCodingRegion = exonRegion.Intersect(codingRegion);
                if (exonCodingRegion.Start == -1) continue;
                cdsIntervals.Add(new Interval(exonCodingRegion.Start, exonCodingRegion.End));
            }
            return cdsIntervals.ToArray();
        }
    }
}