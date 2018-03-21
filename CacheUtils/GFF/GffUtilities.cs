using System;
using System.Collections.Generic;
using System.Linq;
using CacheUtils.Genes.Utilities;
using VariantAnnotation.Algorithms;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;

namespace CacheUtils.GFF
{
    public static class GffUtilities
    {
        public static bool HasCds(IInterval codingRegion, IInterval exon)
        {
            if (codingRegion == null || codingRegion.Start == -1 || codingRegion.End == -1) return false;
            return exon.Overlaps(codingRegion);
        }

        public static IInterval GetCdsCoordinates(IInterval codingRegion, ITranscriptRegion exon)
        {
            int start = exon.Start;
            int end   = exon.End;

            if (start < codingRegion.Start) start = codingRegion.Start;
            if (end   > codingRegion.End)   end   = codingRegion.End;

            return new Interval(start, end);
        }

        public static bool HasUtr(IInterval codingRegion, IInterval exon)
        {
            if (codingRegion == null || codingRegion.Start == -1 || codingRegion.End == -1) return false;
            return exon.Start < codingRegion.Start || exon.End > codingRegion.End;
        }

        public static IEnumerable<ITranscriptRegion> GetExons(this ITranscriptRegion[] regions) =>
            regions.FilterNonExons().Merge().OrderBy(x => x.Start).ThenBy(x => x.End);

        private static ITranscriptRegion[] FilterNonExons(this IEnumerable<ITranscriptRegion> regions) =>
            regions.Where(region => region.Type == TranscriptRegionType.Exon).ToArray();

        private static IEnumerable<ITranscriptRegion> Merge(this IReadOnlyCollection<ITranscriptRegion> exons)
        {
            if (exons.Count == 1) return exons;

            var mergedExons = new List<ITranscriptRegion>();
            var exonsById   = exons.GetMultiValueDict(x => x.Id);

            foreach (var kvp in exonsById)
            {
                mergedExons.Add(MergeTranscriptRegions(kvp.Key, kvp.Value));
            }

            return mergedExons;
        }

        private static ITranscriptRegion MergeTranscriptRegions(ushort exonId, IReadOnlyList<ITranscriptRegion> regions)
        {
            if (regions.Count == 1) return regions[0];

            int lastIndex = regions.Count - 1;

            int start     = regions[0].Start;
            int end       = regions[lastIndex].End;
            int cdnaStart = Math.Min(regions[0].CdnaStart, regions[lastIndex].CdnaStart);
            int cdnaEnd   = Math.Max(regions[0].CdnaEnd, regions[lastIndex].CdnaEnd);

            return new TranscriptRegion(TranscriptRegionType.Exon, exonId, start, end, cdnaStart, cdnaEnd);
        }
    }
}
