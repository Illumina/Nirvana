using System.Collections.Generic;
using VariantAnnotation.DataStructures.Annotation;
using VariantAnnotation.DataStructures.Intervals;
using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.Algorithms
{
    public static class CdnaMapper
    {
        /// <summary>
        /// maps the genomic coordinates to cDNA coordinates.
        /// </summary>
        private static List<Coordinate> MapInternalCoordinates(int start, int end, Transcript transcript)
        {
            var result = new List<Coordinate>();
            var onReverseStrand = transcript.Gene.OnReverseStrand;

            // sanity check: make sure we have coordinate maps
            if (transcript.CdnaMaps == null)
            {
                result.Add(new Coordinate(start, end, true));
                return result;
            }

            int startIdx = 0;
            int endIdx = transcript.CdnaMaps.Length - 1;

            while (startIdx <= endIdx)
            {
                if (transcript.CdnaMaps[startIdx].GenomicEnd >= start) break;
                startIdx++;
            }

            for (var i = startIdx; i < transcript.CdnaMaps.Length; i++)
            {
                var map = transcript.CdnaMaps[i];

                if (end <= map.GenomicEnd)
                {
                    if (end < map.GenomicStart)
                    {
                        result.Add(new Coordinate(start, end, true));
                    }
                    else
                    {
                        if (start < map.GenomicStart)
                        {
                            result.Add(new Coordinate(start, map.GenomicStart - 1, true));
                            result.Add(ConvertGenomicPosToCdnaPos(map.GenomicStart, end, map, onReverseStrand));
                        }
                        else
                        {
                            result.Add(ConvertGenomicPosToCdnaPos(start, end, map, onReverseStrand));
                        }
                    }
                    break;
                }

                if (start < map.GenomicStart)
                {
                    result.Add(new Coordinate(start, map.GenomicStart - 1, true));
                    result.Add(new Coordinate(map.CdnaStart, map.CdnaEnd, false));
                }
                else
                {
                    result.Add(ConvertGenomicPosToCdnaPos(start, map.GenomicEnd, map, onReverseStrand));
                }

                start = map.GenomicEnd + 1;
            }

            // process the last part
            if (end > transcript.CdnaMaps[transcript.CdnaMaps.Length - 1].GenomicEnd)
            {
                result.Add(new Coordinate(start, end, true));
            }

            if (transcript.Gene.OnReverseStrand) result.Reverse();
            return result;
        }

        private static Coordinate ConvertGenomicPosToCdnaPos(int start, int end, CdnaCoordinateMap map, bool onReverseStrand)
        {
            int cdnaStart;
            int cdnaEnd;

            if (onReverseStrand)
            {
                cdnaStart = map.CdnaStart - end + map.GenomicEnd;
                cdnaEnd = map.CdnaStart - start + map.GenomicEnd;
            }
            else
            {
                cdnaStart = start - map.GenomicStart + map.CdnaStart;
                cdnaEnd = end - map.GenomicEnd + map.CdnaEnd;
            }

            return new Coordinate(cdnaStart, cdnaEnd, false);
        }

        public static void MapCoordinates(int start, int end, TranscriptAnnotation ta, Transcript transcript)
        {
            var isInsertion = start > end;

            if (isInsertion) Swap.Int(ref start, ref end);

            var results = MapInternalCoordinates(start, end, transcript);

            var coords = results;
            if (isInsertion) coords = SetInsertionCdna(results, transcript);

            var first = coords[0];
            var last = coords[coords.Count - 1];

            ta.HasValidCdnaStart = !first.IsGap;
            ta.HasValidCdnaEnd = !last.IsGap;

            ta.ComplementaryDnaBegin = first.IsGap ? -1 : first.Start;
            ta.ComplementaryDnaEnd = last.IsGap ? -1 : last.End;

            // grab the backup coordinates
            AssignBackupCdnaPositions(coords, ta);
        }

        private static List<Coordinate> SetInsertionCdna(List<Coordinate> coords, Transcript transcript)
        {
            var result = new List<Coordinate>();
            if (coords.Count == 1)
            {
                var coord = coords[0];
                Swap.Int(ref coord.Start, ref coord.End);
                result.Add(coord);
                return result;
            }

            // insertion on the boundary of gap
            var first = coords[0];
            var last = coords[coords.Count - 1];

            if (!first.IsGap)
            {
                first.Start++;
                if (first.Start > transcript.TotalExonLength || first.End < 1) first.IsGap = true;

                result.Add(first);
            }

            if (!last.IsGap)
            {
                last.End--;
                if (last.Start > transcript.TotalExonLength || last.End < 1) last.IsGap = true;
                result.Add(last);
            }

            return result;
        }

        /// <summary>
        /// assigns the backup cDNA positions (ignoring gaps)
        /// </summary>
        private static void AssignBackupCdnaPositions(List<Coordinate> coords, TranscriptAnnotation ta)
        {
            var nonGaps = new List<Coordinate>();
            foreach (var coord in coords) if (!coord.IsGap) nonGaps.Add(coord);

            if (nonGaps.Count == 0)
            {
                ta.BackupCdnaBegin = -1;
                ta.BackupCdnaEnd = -1;
                return;
            }

            ta.BackupCdnaBegin = nonGaps[0].Start;
            ta.BackupCdnaEnd = nonGaps[nonGaps.Count - 1].End;
        }

        private sealed class Coordinate : AnnotationInterval
        {
            public bool IsGap;

            /// <summary>
            /// constructor
            /// </summary>
            public Coordinate(int start, int end, bool isGap) : base(start, end)
            {
                IsGap = isGap;
            }
        }
    }
}
