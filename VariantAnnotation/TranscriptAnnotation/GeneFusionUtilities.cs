using System;
using System.Collections.Generic;
using Intervals;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.TranscriptAnnotation
{
    public static class GeneFusionUtilities
    {
        internal static void GetGeneFusionsByTranscript(this Dictionary<string, IAnnotatedGeneFusion> transcriptIdToGeneFusions,
            BreakEndAdjacency adjacency, ITranscript[] originTranscripts, ITranscript[] partnerTranscripts)
        {
            var geneFusions = new List<IGeneFusion>();

            foreach (var originTranscript in originTranscripts)
            {
                geneFusions.Clear();
                bool originOnReverseStrand = originTranscript.Gene.OnReverseStrand ^ adjacency.Origin.OnReverseStrand;
                (int originIndex, ITranscriptRegion originRegion) = MappedPositionUtilities.FindRegion(originTranscript.TranscriptRegions, adjacency.Origin.Position);

                int? originExon   = originRegion.Type == TranscriptRegionType.Exon ? (int?) originRegion.Id : null;
                int? originIntron = originRegion.Type == TranscriptRegionType.Intron ? (int?) originRegion.Id : null;
                
                foreach (var partnerTranscript in partnerTranscripts)
                {
                    bool partnerOnReverseStrand      = partnerTranscript.Gene.OnReverseStrand ^ adjacency.Partner.OnReverseStrand;
                    bool differentStrand             = originOnReverseStrand != partnerOnReverseStrand;
                    bool differentTranscriptSource   = originTranscript.Source != partnerTranscript.Source;
                    bool sameGeneSymbol              = originTranscript.Gene.Symbol == partnerTranscript.Gene.Symbol;
                    bool codingRegionAlreadyOverlaps = originTranscript.Translation.CodingRegion.Overlaps(partnerTranscript.Translation.CodingRegion);

                    if (differentStrand || differentTranscriptSource || sameGeneSymbol || codingRegionAlreadyOverlaps) continue;

                    (int partnerIndex, ITranscriptRegion partnerRegion) = MappedPositionUtilities.FindRegion(partnerTranscript.TranscriptRegions, adjacency.Partner.Position);

                    int? partnerExon   = partnerRegion.Type == TranscriptRegionType.Exon ? (int?) partnerRegion.Id : null;
                    int? partnerIntron = partnerRegion.Type == TranscriptRegionType.Intron ? (int?) partnerRegion.Id : null;

                    BreakPointTranscript origin  = new BreakPointTranscript(originTranscript, adjacency.Origin.Position, originIndex);
                    BreakPointTranscript partner = new BreakPointTranscript(partnerTranscript, adjacency.Partner.Position, partnerIndex);
                    (BreakPointTranscript first, BreakPointTranscript second) = originOnReverseStrand ? (partner, origin) : (origin, partner);
                    
                    string hgvsCoding = GetHgvsCoding(first, second);

                    geneFusions.Add(new GeneFusion(partnerExon, partnerIntron, hgvsCoding));
                }

                if (geneFusions.Count == 0) continue;
                var annotatedGeneFusion = new AnnotatedGeneFusion(originExon, originIntron, geneFusions.ToArray());
                transcriptIdToGeneFusions[originTranscript.Id.WithVersion] = annotatedGeneFusion;
            }
        }

        internal static string GetHgvsCoding(BreakPointTranscript first, BreakPointTranscript second)
        {
            (string firstBegin, string firstEnd)   = AdjustFirst(first);
            (string secondBegin, string secondEnd) = AdjustSecond(second);

            return $"{first.Transcript.Gene.Symbol}{{{first.Transcript.Id.WithVersion}}}:c.{firstBegin}_{firstEnd}_{second.Transcript.Gene.Symbol}{{{second.Transcript.Id.WithVersion}}}:c.{secondBegin}_{secondEnd}";
        }

        private static (string Begin, string End) AdjustFirst(BreakPointTranscript first)
        {
            var codingRegion       = first.Transcript.Translation.CodingRegion;
            int position           = first.GenomicPosition;
            int codingRegionLength = codingRegion.CdnaEnd - codingRegion.CdnaStart + 1;

            if (position < codingRegion.Start) return ("?", (position - codingRegion.Start).ToString());
            if (position > codingRegion.End) return ("?", (position - codingRegion.End + codingRegionLength).ToString());
            return ("1", HgvsUtilities.GetCdnaPositionOffset(first.Transcript, position, first.RegionIndex).Value);
        }

        private static (string Begin, string End) AdjustSecond(BreakPointTranscript second)
        {
            var codingRegion       = second.Transcript.Translation.CodingRegion;
            int position           = second.GenomicPosition;
            int codingRegionLength = codingRegion.CdnaEnd - codingRegion.CdnaStart + 1;

            if (position < codingRegion.Start) return ((position - codingRegion.Start).ToString(), "?");
            if (position > codingRegion.End) return ((position - codingRegion.End + codingRegionLength).ToString(), "?");
            return (HgvsUtilities.GetCdnaPositionOffset(second.Transcript, position, second.RegionIndex).Value, codingRegionLength.ToString());
        }
    }
}
