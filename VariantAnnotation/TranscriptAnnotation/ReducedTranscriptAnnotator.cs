using System;
using System.Collections.Generic;
using VariantAnnotation.Algorithms;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.Consequence;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;

namespace VariantAnnotation.TranscriptAnnotation
{
    public static class ReducedTranscriptAnnotator
    {
        public static IAnnotatedTranscript GetAnnotatedTranscript(ITranscript transcript, IVariant variant, IEnumerable<ITranscript> geneFusionCandidates)
        {
            var exonsAndIntrons = MappedPositionsUtils.ComputeExonAndIntron(variant.Start, variant.End, transcript.CdnaMaps, transcript.Introns, transcript.Gene.OnReverseStrand);

            var nullInterval = new NullableInterval(null, null);
            var mappedPosition = new MappedPositions(nullInterval, null, nullInterval, null, nullInterval, exonsAndIntrons.Item1, exonsAndIntrons.Item2);

            var geneFusionAnnotation = ComputeGeneFusions(variant.BreakEnds, transcript, geneFusionCandidates);

            var featureEffect = new FeatureVariantEffects(transcript, variant.Type, variant.Start, variant.End, true);

            var consequence = new Consequences(null, featureEffect);
            consequence.DetermineStructuralVariantEffect(geneFusionAnnotation != null, variant.Type);


            return new AnnotatedTranscript(transcript, null, null, null, null, mappedPosition, null, null, null, null, consequence.GetConsequences(), geneFusionAnnotation);
        }

        internal static IGeneFusionAnnotation ComputeGeneFusions(IBreakEnd[] breakEnds, ITranscript transcript,
            IEnumerable<ITranscript> geneFusionCandidates)
        {
            if (transcript.Translation == null || breakEnds == null || breakEnds.Length == 0) return null;
            var breakendToAnnotate = FindBreakEndToAnnotate(breakEnds, transcript);

            if (breakendToAnnotate == null) return null;
            var position1Info =
                ComputeBreakendTranscriptRelation(transcript, breakendToAnnotate.Position1,
                    breakendToAnnotate.IsSuffix1);

            var geneFusions = new List<IGeneFusion>();
            foreach (var candidate in geneFusionCandidates)
            {
                var geneFusion = GetFusion(candidate, transcript, breakendToAnnotate, position1Info.Item3, position1Info.Item4);
                if (geneFusion != null) geneFusions.Add(geneFusion);
            }

            return geneFusions.Count == 0
                ? null
                : new GeneFusionAnnotation(position1Info.Item1, position1Info.Item2, geneFusions);
        }

        /// <summary>
        /// evaluate if a candidate transcript can lead to a gene fusion if satisfy
        /// -- transcript coding region do not overlap
        /// -- have the same transcript source
        /// -- have different gene name
        /// -- breakendPosition 2 falls to coding region
        /// -- unidirectional fusion with the other gene
        /// </summary>
        /// <param name="candidateForPos2"></param>
        /// <param name="transcriptForPos1"></param>
        /// <param name="breakendToAnnotate"></param>
        /// <param name="pos1Hgvs"></param>
        /// <param name="isPos1TranscriptSuffix"></param>
        /// <returns></returns>
        private static IGeneFusion GetFusion(ITranscript candidateForPos2, ITranscript transcriptForPos1, IBreakEnd breakendToAnnotate, string pos1Hgvs, bool isPos1TranscriptSuffix)
        {
            if (transcriptForPos1.Source != candidateForPos2.Source
                || candidateForPos2.Translation == null ||
                candidateForPos2.Gene.Symbol == transcriptForPos1.Gene.Symbol ||
                candidateForPos2.Chromosome.Index == transcriptForPos1.Chromosome.Index &&
                candidateForPos2.Translation.CodingRegion.Overlaps(transcriptForPos1.Translation.CodingRegion) ||
                candidateForPos2.Chromosome.Index != breakendToAnnotate.Chromosome2.Index
                || !candidateForPos2.Translation.CodingRegion.Overlaps(breakendToAnnotate.Position2,
                    breakendToAnnotate.Position2)) return null;

            var pos2Info = ComputeBreakendTranscriptRelation(candidateForPos2, breakendToAnnotate.Position2,
                breakendToAnnotate.IsSuffix2);
            if (pos2Info.Item4 == isPos1TranscriptSuffix) return null;
            var hgvsString = isPos1TranscriptSuffix ? pos2Info.Item3 + "_" + pos1Hgvs : pos1Hgvs + "_" + pos2Info.Item3;

            return new GeneFusion(pos2Info.Item1, pos2Info.Item2, hgvsString);
        }

        private static (int? Exon, int? Intron, string Hgvs, bool IsTranscriptSuffix) ComputeBreakendTranscriptRelation(ITranscript transcript, int position, bool isGenomicSuffix)
        {
            var positionOffset = HgvsUtilities.GetCdnaPositionOffset(transcript, position, true);
            var exon = GetIntervalIndex(transcript.CdnaMaps, position, transcript.Gene.OnReverseStrand);
            var intron = GetIntervalIndex(transcript.Introns, position, transcript.Gene.OnReverseStrand);
            var isTranscriptSuffix = isGenomicSuffix != transcript.Gene.OnReverseStrand;
            var transcriptCdnaLength = transcript.Translation.CodingRegion.CdnaEnd - transcript.Translation.CodingRegion.CdnaStart + 1;
            var hgvsPosString = isTranscriptSuffix ? positionOffset.Value + "_" + transcriptCdnaLength : 1 + "_" + positionOffset.Value;

            var hgvs = transcript.Gene.Symbol + "{" + transcript.GetVersionedId() + "}" + ":c." + hgvsPosString;
            return (exon, intron, hgvs, isTranscriptSuffix);
        }

        private static int? GetIntervalIndex(IInterval[] intervals, int position, bool onReverseStrand)
        {
            if (intervals == null || intervals.Length == 0) return null;
            for (var i = 0; i < intervals.Length; i++)
            {
                if (intervals[i].Start <= position && intervals[i].End >= position)
                    return onReverseStrand ? intervals.Length - i : i + 1;
            }

            return null;
        }

        /// <summary>
        /// Identify the candidate breakend to annotate for current transcript
        /// satisfy:breakend position 1 overlaps with transcript coding region
        /// </summary>
        /// <param name="breakEnds"></param>
        /// <param name="transcript"></param>
        /// <returns></returns>
        private static IBreakEnd FindBreakEndToAnnotate(IBreakEnd[] breakEnds, ITranscript transcript)
        {
            foreach (var breakEnd in breakEnds)
            {
                if (transcript.Chromosome.Index != breakEnd.Chromosome1.Index ||
                    breakEnd.Position1 < transcript.Translation.CodingRegion.Start ||
                    breakEnd.Position1 > transcript.Translation.CodingRegion.End) continue;

                return breakEnd;
            }

            return null;
        }
    }
}