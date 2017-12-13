using System;
using System.Collections.Generic;
using VariantAnnotation.Algorithms;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public static class MappedPositionsUtils
    {
        public static IMappedPositions ComputeMappedPositions(int start, int end, ITranscript transcript)
        {
            var exonIntron = ComputeExonAndIntron(start, end, transcript.CdnaMaps, transcript.Introns,
                transcript.Gene.OnReverseStrand);

            var coords = MapCdnaInterval(start, end, transcript);

            var first = coords[0];
            var last  = coords[coords.Count - 1];

            var cdnaInterval = new NullableInterval(first.IsGap ? (int?)null : first.Start,
                last.IsGap ? (int?)null : last.End);

            var impactedCdnaInterval = AssignBackupCdnaPositions(coords);

            (NullableInterval CdsInterval, IInterval ImpactedCdsInterval) cdsIntervals = (new NullableInterval(null, null), null); 
            if (transcript.Translation != null)
                 cdsIntervals = UpdateCdsPosition(transcript.Translation, transcript.StartExonPhase, impactedCdnaInterval.Start,
                     impactedCdnaInterval.End, first.IsGap,last.IsGap);

            var proteinInterval = UpdateProteinPosition(cdsIntervals.CdsInterval);

            return new MappedPositions(cdnaInterval, impactedCdnaInterval, cdsIntervals.CdsInterval, cdsIntervals.ImpactedCdsInterval, proteinInterval,exonIntron.AffectedExons,exonIntron.AffectedIntrons);
        }

        public static (IInterval AffectedExons, IInterval AffectedIntrons) ComputeExonAndIntron(int start, int end,
            ICdnaCoordinateMap[] cdnaCoordinateMaps, IInterval[] introns, bool onReverseStrand)
        {
            var numExons                = cdnaCoordinateMaps.Length;
            var overlappedExonIndices   = new List<int>();
            var overlappedIntronIndices = new List<int>();
            var variantInterval         = new Interval(start, end);

            for (var i = 0; i < numExons; i++)
            {
                if (variantInterval.Overlaps(cdnaCoordinateMaps[i])) overlappedExonIndices.Add(i);
            }

            if (introns != null)
            {
                for (var j = 0; j < introns.Length; j++)
                {
                    if (variantInterval.Overlaps(introns[j])) overlappedIntronIndices.Add(j);
                }
            }
            IInterval affectedExons = null, affectedIntrons = null;
            if (overlappedExonIndices.Count > 0)
            {
                if (onReverseStrand)
                {
                    affectedExons =
                        new Interval(numExons - overlappedExonIndices[overlappedExonIndices.Count - 1],
                            numExons - overlappedExonIndices[0]);
                }
                else
                {
                    affectedExons = new Interval(overlappedExonIndices[0] + 1,
                        overlappedExonIndices[overlappedExonIndices.Count - 1] + 1);
                }
            }

            if (introns != null && overlappedIntronIndices.Count > 0)
            {
                if (onReverseStrand)
                {
                    affectedIntrons =
                        new Interval(introns.Length - overlappedIntronIndices[overlappedIntronIndices.Count - 1],
                            introns.Length - overlappedIntronIndices[0]);
                }
                else
                {
                    affectedIntrons = new Interval(overlappedIntronIndices[0] + 1,
                        overlappedIntronIndices[overlappedIntronIndices.Count - 1] + 1);
                }
            }

            return (affectedExons, affectedIntrons);
        }

        private static NullableInterval UpdateProteinPosition(NullableInterval cdsInterval)
        {
            const int shift = 0;

            var start = cdsInterval.Start == null
                ? null
                : (cdsInterval.Start + shift + 2) / 3;

            var end = cdsInterval.End == null
                ? null
                : (cdsInterval.End + shift + 2) / 3;

            return new NullableInterval(start, end);
        }

        private static List<MappedPositions.Coordinate> MapCdnaInterval(int start, int end, ITranscript transcript)
        {
            var isInsertion = start > end;

            if (isInsertion) Swap.Int(ref start, ref end);

            var results = MapGenomicToCodingCoordinates(start, end, transcript.CdnaMaps, transcript.Gene.OnReverseStrand);

            var coords = results;
            if (isInsertion) coords = SetInsertionCdna(results, transcript.TotalExonLength);

            return coords;
        }

        /// <summary>
        /// maps the genomic coordinates to cDNA coordinates.
        /// </summary>
        private static List<MappedPositions.Coordinate> MapGenomicToCodingCoordinates(int start, int end, ICdnaCoordinateMap[] cdnaMaps,
            bool onReverseStrand)
        {
            var result = new List<MappedPositions.Coordinate>();

            // sanity check: make sure we have coordinate maps
            if (cdnaMaps == null)
            {
                result.Add(new MappedPositions.Coordinate(start, end, true));
                return result;
            }

            var startIdx = 0;
            var endIdx = cdnaMaps.Length - 1;

            while (startIdx <= endIdx)
            {
                if (cdnaMaps[startIdx].End >= start) break;
                startIdx++;
            }

            for (var i = startIdx; i < cdnaMaps.Length; i++)
            {
                var map = cdnaMaps[i];

                if (end <= map.End)
                {
                    if (end < map.Start)
                    {
                        result.Add(new MappedPositions.Coordinate(start, end, true));
                    }
                    else
                    {
                        if (start < map.Start)
                        {
                            result.Add(new MappedPositions.Coordinate(start, map.Start - 1, true));
                            result.Add(GenomicToCdnaPos(map.Start, end, map, onReverseStrand));
                        }
                        else
                        {
                            result.Add(GenomicToCdnaPos(start, end, map, onReverseStrand));
                        }
                    }
                    break;
                }

                if (start < map.Start)
                {
                    result.Add(new MappedPositions.Coordinate(start, map.Start - 1, true));
                    result.Add(new MappedPositions.Coordinate(map.CdnaStart, map.CdnaEnd, false));
                }
                else
                {
                    result.Add(GenomicToCdnaPos(start, map.End, map, onReverseStrand));
                }

                start = map.End + 1;
            }

            // process the last part
            if (end > cdnaMaps[cdnaMaps.Length - 1].End)
            {
                result.Add(new MappedPositions.Coordinate(start, end, true));
            }

            if (onReverseStrand) result.Reverse();
            return result;
        }

        private static MappedPositions.Coordinate GenomicToCdnaPos(int start, int end, ICdnaCoordinateMap map,
            bool onReverseStrand)
        {
            int cdnaStart;
            int cdnaEnd;

            if (onReverseStrand)
            {
                cdnaStart = map.CdnaStart - end + map.End;
                cdnaEnd   = map.CdnaStart - start + map.End;
            }
            else
            {
                cdnaStart = start - map.Start + map.CdnaStart;
                cdnaEnd   = end - map.End + map.CdnaEnd;
            }

            return new MappedPositions.Coordinate(cdnaStart, cdnaEnd, false);
        }

        private static List<MappedPositions.Coordinate> SetInsertionCdna(List<MappedPositions.Coordinate> coords, int totalExonLength)
        {
            var result = new List<MappedPositions.Coordinate>();

            if (coords.Count == 1)
            {
                var coord = coords[0];
                Swap.Int(ref coord.Start, ref coord.End);
                result.Add(coord);
                return result;
            }

            // insertion on the boundary of gap
            var first = coords[0];
            var last  = coords[coords.Count - 1];

            if (!first.IsGap)
            {
                first.Start++;
                if (first.Start > totalExonLength || first.End < 1) first.IsGap = true;

                result.Add(first);
            }

            if (!last.IsGap)
            {
                last.End--;
                if (last.Start > totalExonLength || last.End < 1) last.IsGap = true;
                result.Add(last);
            }

            return result;
        }

        /// <summary>
        /// assigns the backup cDNA positions (ignoring gaps)
        /// </summary>
        private static IInterval AssignBackupCdnaPositions(List<MappedPositions.Coordinate> coords)
        {
            var nonGaps = new List<MappedPositions.Coordinate>();
            foreach (var coord in coords) if (!coord.IsGap) nonGaps.Add(coord);

            return nonGaps.Count == 0 ? new Interval(-1, -1) : new Interval(nonGaps[0].Start, nonGaps[nonGaps.Count - 1].End);
        }

        private static (NullableInterval CdsInterval, IInterval ImpactedCdsInterval) UpdateCdsPosition(ITranslation translation,
            byte startExonPhase, int impactedCdnaStart, int impactedCdnaEnd,bool invalidCdnaStart, bool invalidCdnaEnd)
        {
            var beginOffset = startExonPhase - translation.CodingRegion.CdnaStart + 1;

            if (impactedCdnaEnd < translation.CodingRegion.CdnaStart ||
                impactedCdnaStart > translation.CodingRegion.CdnaEnd ||
                impactedCdnaStart == -1 && impactedCdnaEnd == -1)
                return (new NullableInterval(null,null), null);

            var impactedCdsStart = impactedCdnaStart + beginOffset;
            var impactedCdsEnd = impactedCdnaEnd + beginOffset;

            IInterval impactedCdsInterval = new Interval(impactedCdsStart, impactedCdsEnd);

            var cdsStart = impactedCdsStart < 1 || invalidCdnaStart
                ? (int?)null
                : impactedCdsStart;

            var cdsEnd =
                impactedCdsEnd > translation.CodingRegion.CdnaEnd + beginOffset ||
                invalidCdnaEnd
                    ? (int?)null
                    : impactedCdsEnd;

            var cdsInterval = new NullableInterval(cdsStart, cdsEnd);

            return (cdsInterval, impactedCdsInterval);
        }
    }
}