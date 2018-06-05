using System;
using System.Collections.Generic;
using System.Linq;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using Intervals;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.TranscriptCache
{
    public static class TranscriptRegionMerger
    {
        public static ITranscriptRegion[] GetTranscriptRegions(IEnumerable<MutableTranscriptRegion> cdnaMaps, MutableExon[] exons,
            IInterval[] introns, bool onReverseStrand)
        {
            var sortedRegions = cdnaMaps.OrderBy(x => x.Start).ThenBy(x => x.End).ToList();

            var intronIntervals = introns == null
                ? null
                : CreateIntervals(introns.OrderBy(x => x.Start).ThenBy(x => x.End), introns.Length, onReverseStrand);

            var exonIntervals = CreateIntervals(exons.OrderBy(x => x.Start).ThenBy(x => x.End), exons.Length,
                onReverseStrand);
            
            return sortedRegions.AddGaps()
                .AddIds(intronIntervals, TranscriptRegionType.Gap, TranscriptRegionType.Intron)
                .AddIds(exonIntervals, TranscriptRegionType.Exon, TranscriptRegionType.Exon)
                .AddIds(exonIntervals, TranscriptRegionType.Gap, TranscriptRegionType.Gap)
                .AddCoords(TranscriptRegionType.Intron, onReverseStrand)
                .AddCoords(TranscriptRegionType.Gap, onReverseStrand)
                .ToInterfaceArray();
        }

        private static List<MutableTranscriptRegion> AddCoords(this List<MutableTranscriptRegion> regions, TranscriptRegionType targetRegionType, bool onReverseStrand)
        {
            for (var regionIndex = 0; regionIndex < regions.Count; regionIndex++)
            {
                var region = regions[regionIndex];
                if (region.Type != targetRegionType) continue;
                var coords = regions.GetExonCoords(regionIndex, onReverseStrand);
                region.CdnaStart = coords.CdnaStart;
                region.CdnaEnd   = coords.CdnaEnd;
            }

            return regions;
        }

        private static (int CdnaStart, int CdnaEnd) GetExonCoords(this IReadOnlyList<MutableTranscriptRegion> regions,
            int regionIndex, bool onReverseStrand)
        {
            int cdnaStart = -1;
            int cdnaEnd   = -1;

            int testIndex = regionIndex;
            while (testIndex >= 0)
            {
                testIndex--;
                var region = regions[testIndex];
                if (region.Type != TranscriptRegionType.Exon) continue;
                if (onReverseStrand) cdnaEnd = region.CdnaStart;
                else cdnaStart = region.CdnaEnd;
                break;
            }

            testIndex = regionIndex;
            while (testIndex < regions.Count)
            {
                testIndex++;
                var region = regions[testIndex];
                if (region.Type != TranscriptRegionType.Exon) continue;
                if (onReverseStrand) cdnaStart = region.CdnaEnd;
                else cdnaEnd = region.CdnaStart;
                break;
            }

            return (cdnaStart, cdnaEnd);
        }

        private static ITranscriptRegion[] ToInterfaceArray(this IReadOnlyList<MutableTranscriptRegion> mutableRegions)
        {
            var regions = new ITranscriptRegion[mutableRegions.Count];
            for (var i = 0; i < mutableRegions.Count; i++)
            {
                var region = mutableRegions[i];
                regions[i] = new TranscriptRegion(region.Type, region.Id, region.Start, region.End, region.CdnaStart,
                    region.CdnaEnd);
            }
            return regions;
        }

        private static IdInterval[] CreateIntervals(IEnumerable<IInterval> intervals, int numIntervals, bool onReverseStrand)
        {
            var idIntervals = new IdInterval[numIntervals];
            ushort id       = onReverseStrand ? (ushort)numIntervals : (ushort)1;
            var index       = 0;

            foreach (var interval in intervals)
            {
                idIntervals[index] = new IdInterval(interval.Start, interval.End, id);
                if (onReverseStrand) id--;
                else id++;
                index++;
            }

            return idIntervals.OrderBy(x => x.Start).ThenBy(x => x.End).ToArray();
        }

        private static List<MutableTranscriptRegion> AddIds(this List<MutableTranscriptRegion> regions,
            IReadOnlyList<IdInterval> intervals, TranscriptRegionType targetRegionType, TranscriptRegionType matchRegionType)
        {
            if (intervals == null) return regions;

            foreach (var region in regions)
            {
                if (region.Type != targetRegionType) continue;

                int regionMidPoint = region.Start + (region.End - region.Start >> 1);

                int index = intervals.BinarySearch(regionMidPoint);
                if (index < 0) continue;

                var intron  = intervals[index];
                region.Type = matchRegionType;
                region.Id   = intron.Id;
            }

            return regions;
        }

        private static int BinarySearch(this IReadOnlyList<IdInterval> intervals, int position)
        {
            var begin = 0;
            int end = intervals.Count - 1;

            while (begin <= end)
            {
                int index = begin + (end - begin >> 1);
                var interval = intervals[index];

                if (position >= interval.Start && position <= interval.End) return index;
                if (interval.End < position) begin = index + 1;
                else if (position < interval.Start) end = index - 1;
            }

            return ~begin;
        }

        private static List<MutableTranscriptRegion> AddGaps(this List<MutableTranscriptRegion> sortedRegions)
        {
            for (var i = 1; i < sortedRegions.Count; i++)
            {
                var prevRegion = sortedRegions[i - 1];
                var region     = sortedRegions[i];

                int gapLength = CalculateGapLength(prevRegion, region);
                if (gapLength == 0) continue;

                var gapRegion = new MutableTranscriptRegion(TranscriptRegionType.Gap, 0, prevRegion.End + 1, region.Start - 1);
                sortedRegions.Insert(i, gapRegion);
                i++;
            }

            return sortedRegions;
        }

        private static int CalculateGapLength(IInterval prevRegion, IInterval region) => region.Start - prevRegion.End - 1;

        private sealed class IdInterval : IInterval, IComparable<IdInterval>
        {
            public int Start { get; }
            public int End { get; }
            public readonly ushort Id;

            public IdInterval(int start, int end, ushort id)
            {
                Start = start;
                End   = end;
                Id    = id;
            }

            public int CompareTo(IdInterval other)
            {
                if (ReferenceEquals(this, other)) return 0;
                if (ReferenceEquals(null, other)) return 1;

                int startComparison = Start.CompareTo(other.Start);
                return startComparison != 0 ? startComparison : End.CompareTo(other.End);
            }
        }
    }
}
