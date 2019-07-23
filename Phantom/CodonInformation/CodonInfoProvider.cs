using System;
using System.Collections.Generic;
using System.Linq;
using Genome;
using Intervals;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Utilities;

namespace Phantom.CodonInformation
{
    public sealed class CodonInfoProvider : ICodonInfoProvider
    {
        private IntervalArray<ICodingBlock> _commonCodingBlockArray;
        private int _currentChrIndex = -1;
        private readonly IntervalArray<ITranscript>[] _transcriptIntervalArrays;
        // ReSharper disable once NotAccessedField.Local

        public CodonInfoProvider(IntervalArray<ITranscript>[] transcriptIntervalArrays)
        {
            _transcriptIntervalArrays = transcriptIntervalArrays;
        }

        private void UpdateCodingBlockArray(int chrIndex)
        {
            if (chrIndex == _currentChrIndex) return;
            _currentChrIndex = chrIndex;
            _commonCodingBlockArray = null;

            if (chrIndex >= _transcriptIntervalArrays.Length) return;
            var transcriptIntervalArray = _transcriptIntervalArrays[chrIndex];
            if (transcriptIntervalArray == null) return;

            var geneCdsIntervals = GetPhasedCdsIntervals(transcriptIntervalArray);
            var intervalsWithPhase = new List<Interval<ICodingBlock>>();

            foreach (var (gene, transcriptIntervals) in geneCdsIntervals)
            {
                var transcriptToCodingBlocks =
                    GetTranscriptToCodingBlocks(transcriptIntervals, gene.OnReverseStrand);
                intervalsWithPhase.AddRange(GetIntervalsWithPhase(transcriptToCodingBlocks));
            }
            _commonCodingBlockArray = new IntervalArray<ICodingBlock>(intervalsWithPhase.OrderBy(x => x.Begin).ToArray());
        }

        private static IEnumerable<Interval<ICodingBlock>> GetIntervalsWithPhase(CodingBlock[][] transcriptToCodingBlocks)
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
            for (var i = 1; i < functionBlockRanges.Length; i++)
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
            for (var i = 0; i < overlappingCodingBlocks.Length; i++)
            {
                var overlappingCodingBlock = overlappingCodingBlocks[i];
                functionBlockRanges[i] = GetFunctionBlockRange(chrInterval, overlappingCodingBlock);
            }
            return functionBlockRanges;
        }

        private ICodingBlock[] GetOverlappingCodingBlocks(IChromosomeInterval chrInterval)
        {
            UpdateCodingBlockArray(chrInterval.Chromosome.Index);
            return _commonCodingBlockArray?.GetAllOverlappingValues(chrInterval.Start, chrInterval.End);
        }
            
        private static int GetFunctionBlockRange(IInterval interval, ICodingBlock overlappingCodingBlock)
        {
            // only check codon boundary in the same exon for now
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
                if (transcript.Id.IsPredictedTranscript()) continue;

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

        internal static CodingBlock[][] GetTranscriptToCodingBlocks(List<PhasedIntervalArray> transcriptIntervals, bool onReverseStrand)
        {
            var transcriptToCommonIntervals = IntervalPartitioner.GetCommonIntervals(new TranscriptIntervalsInGene(transcriptIntervals.Select(x => x.IntervalArray).ToArray()));
            var startPhases = transcriptIntervals.Select(x => x.StartPhase).ToArray();
            var codingBlockArrays = new CodingBlock[transcriptToCommonIntervals.Length][];
            if (onReverseStrand)
            {
                startPhases = GetStartPhaseForReverseTranscripts(transcriptToCommonIntervals, startPhases);
            }

            for (var transcriptId = 0; transcriptId < transcriptToCommonIntervals.Length; transcriptId++)
            {
                byte currentPhase = startPhases[transcriptId];
                var commonIntervals = transcriptToCommonIntervals[transcriptId];
                codingBlockArrays[transcriptId] = new CodingBlock[commonIntervals.Count];
                for (var intervalId = 0; intervalId < commonIntervals.Count; intervalId++)
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

        private static byte[] GetStartPhaseForReverseTranscripts(IReadOnlyList<List<IInterval>> transcriptToIntervals, byte[] startPhases)
        {
            var startPhasesFromEndOfReverseTranscripts = new byte[transcriptToIntervals.Count];
            for (var i = 0; i < transcriptToIntervals.Count; i++)
            {
                int totalLength = GetTotalIntervalLength(transcriptToIntervals[i]);
                startPhasesFromEndOfReverseTranscripts[i] = (byte)(2 - (startPhases[i] + totalLength % 3 + 2) % 3);
            }
            return startPhasesFromEndOfReverseTranscripts;
        }

        private static int GetTotalIntervalLength(IEnumerable<IInterval> transcriptToInterval)
        {
            var totalLength = 0;
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
                var exonCodingRegion = exonRegion.Intersects(codingRegion);
                if (exonCodingRegion.Start == -1) continue;
                cdsIntervals.Add(new Interval(exonCodingRegion.Start, exonCodingRegion.End));
            }
            return cdsIntervals.ToArray();
        }
    }
}