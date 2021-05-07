using System.Collections.Generic;
using Genome;
using Intervals;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;

namespace VariantAnnotation.TranscriptAnnotation
{
    public static class GeneFusionUtilities
    {
        internal static IGeneFusionAnnotation GetGeneFusionAnnotation(IBreakEnd[] breakEnds, ITranscript transcript,
            ITranscript[] fusedTranscriptCandidates)
        {
            if (transcript.Translation == null || breakEnds == null || breakEnds.Length == 0) return null;

            var desiredBreakEnd = GetBreakEndWithinCodingRegion(breakEnds, transcript.Chromosome, transcript.Translation.CodingRegion);
            if (desiredBreakEnd == null) return null;

            var piece1  = MappedPositionUtilities.FindRegion(transcript.TranscriptRegions, desiredBreakEnd.Piece1.Position);
            int? exon   = piece1.Region.Type == TranscriptRegionType.Exon   ? (int?)piece1.Region.Id : null;
            int? intron = piece1.Region.Type == TranscriptRegionType.Intron ? (int?)piece1.Region.Id : null;

            var piece1Hgvs = GetBreakEndHgvs(transcript, piece1.Index, desiredBreakEnd.Piece1.Position, desiredBreakEnd.Piece1.IsSuffix);

            var geneFusions = new List<IGeneFusion>();
            foreach (var candidate in fusedTranscriptCandidates)
            {
                var piece2     = MappedPositionUtilities.FindRegion(candidate.TranscriptRegions, desiredBreakEnd.Piece2.Position);
                var geneFusion = GetGeneFusion(transcript, candidate, desiredBreakEnd.Piece2, piece2.Index,
                    piece1Hgvs.Hgvs, piece1Hgvs.IsTranscriptSuffix);

                if (geneFusion != null) geneFusions.Add(geneFusion);
            }

            return geneFusions.Count == 0
                ? null
                : new GeneFusionAnnotation(exon, intron, geneFusions.ToArray());
        }

        private static IBreakEnd GetBreakEndWithinCodingRegion(IBreakEnd[] breakEnds, IChromosome chromosome,
            IInterval codingRegion)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var breakend in breakEnds)
            {
                var position = breakend.Piece1.Position;
                if (breakend.Piece1.Chromosome != chromosome || position < codingRegion.Start || position > codingRegion.End) continue;
                return breakend;
            }

            return null;
        }

        private static (string Hgvs, bool IsTranscriptSuffix) GetBreakEndHgvs(ITranscript transcript, int regionIndex,
            int position, bool isGenomicSuffix)
        {
            var positionOffset     = HgvsUtilities.GetCdnaPositionOffset(transcript, position, regionIndex);
            var isTranscriptSuffix = isGenomicSuffix != transcript.Gene.OnReverseStrand;
            var codingRegionLength = transcript.Translation.CodingRegion.CdnaEnd - transcript.Translation.CodingRegion.CdnaStart + 1;
            var hgvsPosString      = isTranscriptSuffix ? positionOffset.Value + "_" + codingRegionLength : 1 + "_" + positionOffset.Value;

            var hgvs = transcript.Gene.Symbol + "{" + transcript.Id.WithVersion + "}" + ":c." + hgvsPosString;
            return (hgvs, isTranscriptSuffix);
        }

        /// <summary>
        /// evaluate if a candidate transcript can lead to a gene fusion if satisfy
        /// -- transcript coding region do not overlap
        /// -- have the same transcript source
        /// -- have different gene name
        /// -- breakendPosition 2 falls to coding region
        /// -- unidirectional fusion with the other gene
        /// </summary>
        private static IGeneFusion GetGeneFusion(ITranscript transcript, ITranscript transcript2,
            IBreakEndPiece bePiece2, int piece2RegionIndex, string piece1Hgvs, bool isPos1TranscriptSuffix)
        {
            if (SkipGeneFusion(transcript, transcript2, bePiece2)) return null;

            var region       = transcript2.TranscriptRegions[piece2RegionIndex];
            var piece2Hgvs   = GetBreakEndHgvs(transcript2, piece2RegionIndex, bePiece2.Position, bePiece2.IsSuffix);
            if (piece2Hgvs.IsTranscriptSuffix == isPos1TranscriptSuffix) return null;
            
            int? exon   = region.Type == TranscriptRegionType.Exon ? (int?)region.Id : null;
            int? intron = region.Type == TranscriptRegionType.Intron ? (int?)region.Id : null;

            var hgvs = isPos1TranscriptSuffix ? piece2Hgvs.Hgvs + "_" + piece1Hgvs : piece1Hgvs + "_" + piece2Hgvs.Hgvs;

            return new GeneFusion(exon, intron, hgvs);
        }

        private static bool SkipGeneFusion(ITranscript transcript, ITranscript transcript2, IBreakEndPiece piece2)
        {
            return transcript.Source != transcript2.Source                                            ||
                   transcript2.Translation == null                                                    ||
                   transcript2.Gene.Symbol == transcript.Gene.Symbol                                  ||
                   transcript2.Chromosome.Index == transcript.Chromosome.Index                        &&
                   transcript2.Chromosome.Index != piece2.Chromosome.Index                            ||
                   transcript2.Translation.CodingRegion.Overlaps(transcript.Translation.CodingRegion) ||
                   !transcript2.Translation.CodingRegion.Overlaps(piece2.Position, piece2.Position);
        }
    }
}
