using System.Collections.Generic;
using System.Linq;
using VariantAnnotation.DataStructures;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.Algorithms
{
    public static class CdnaMapper
    {
	    private class Coordinate : AnnotationInterval
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

        /// <summary>
        /// maps the genomic coordinates to cDNA coordinates.
        /// </summary>
        private static LinkedList<Coordinate> MapInternalCoordinates(int start, int end, Transcript transcript)
        {
            var result = new LinkedList<Coordinate>();
		    var onReverseStrand = transcript.OnReverseStrand;

            // sanity check: make sure we have coordinate maps
            if (transcript.CdnaCoordinateMaps == null)
            {
                result.AddLast(new Coordinate(start, end, true));
                return result;
            }

			int startIdx = 0;
			int endIdx = transcript.CdnaCoordinateMaps.Length - 1;

		    while (startIdx <= endIdx)
		    {
			    if (transcript.CdnaCoordinateMaps[startIdx].Genomic.End >= start)
				    break;
			    startIdx++;
		    }

            for (var i = startIdx; i < transcript.CdnaCoordinateMaps.Length ; i++)
            {
                var coordinateMap = transcript.CdnaCoordinateMaps[i];
                var genomicCoord = coordinateMap.Genomic;
                var codingCoord = coordinateMap.CodingDna;

	            if (end <= genomicCoord.End)
	            {
		            if (end < genomicCoord.Start)
		            {
			            result.AddLast(new Coordinate(start, end, true));
		            }
		            else
		            {
			            if (start < genomicCoord.Start)
			            {
				            result.AddLast(new Coordinate(start, genomicCoord.Start - 1, true));
				            result.AddLast(ConvertGenomicPosToCdnaPos(genomicCoord.Start, end, coordinateMap, onReverseStrand));
			            }
			            else
			            {
				            result.AddLast(ConvertGenomicPosToCdnaPos(start, end, coordinateMap, onReverseStrand));
			            }
		            }
		            break;
	            }

	            if (start < genomicCoord.Start)
	            {
		            result.AddLast(new Coordinate(start, genomicCoord.Start - 1, true));
		            result.AddLast(new Coordinate(codingCoord.Start, codingCoord.End, false));
	            }
	            else
	            {
		            result.AddLast(ConvertGenomicPosToCdnaPos(start, genomicCoord.End, coordinateMap, onReverseStrand));
	            }
	            start = genomicCoord.End + 1;
            }

			//process the last part
		    if (end > transcript.CdnaCoordinateMaps.Last().Genomic.End)
		    {
			    result.AddLast(new Coordinate(start, end, true));
		    }

		    if (!transcript.OnReverseStrand) return result;

		    var reverseList = new LinkedList<Coordinate>();
		    foreach (var coordinate in result.Reverse())
		    {
			    reverseList.AddLast(coordinate);
		    }
				
		    return reverseList;
        }

		private static Coordinate ConvertGenomicPosToCdnaPos(int start, int end, CdnaCoordinateMap coordinateMap, bool onReverseStrand)
		{
			int cdnaStart ;
			int cdnaEnd ;

			if (onReverseStrand)
			{
				cdnaStart = coordinateMap.CodingDna.Start - end + coordinateMap.Genomic.End;
				cdnaEnd = coordinateMap.CodingDna.Start - start + coordinateMap.Genomic.End;
			}
			else
			{
				cdnaStart = start - coordinateMap.Genomic.Start + coordinateMap.CodingDna.Start;
				cdnaEnd = end - coordinateMap.Genomic.End + coordinateMap.CodingDna.End;
			}
			return new Coordinate(cdnaStart, cdnaEnd,false);
		}



		public static void MapCoordinates(int start, int end, TranscriptAnnotation ta, Transcript transcript)
		{
			bool isInsertion = start > end; 

			if (isInsertion) Swap.Int(ref start, ref end);

			var results = MapInternalCoordinates(start, end, transcript);

			var coords = results;
			if (isInsertion)
				 coords = SetInsertionCdna(results, transcript);

            var first = coords.First.Value;
            var last = coords.Last.Value;

            ta.HasValidCdnaStart = !first.IsGap;
            ta.HasValidCdnaEnd = !last.IsGap;

            ta.ComplementaryDnaBegin = first.IsGap  ? -1 : first.Start;
            ta.ComplementaryDnaEnd = last.IsGap  ? -1 : last.End;

            // grab the backup coordinates
            AssignBackupCdnaPositions(coords, ta);
        }

		private static LinkedList<Coordinate> SetInsertionCdna(LinkedList<Coordinate> coords,Transcript transcript)
		{
			var result = new LinkedList<Coordinate>();
			if (coords.Count == 1)
			{
				var coord = coords.First.Value;
				Swap.Int(ref coord.Start, ref coord.End);
				result.AddLast(coord);
				return result;
			}



			//insertion on the boundary of gap
			var first = coords.First.Value;
			var last = coords.Last.Value;

		    if (!first.IsGap)
			{
				first.Start++;
				if (first.Start > transcript.TotalExonLength || first.End < 1) first.IsGap = true;

				result.AddLast(first);
			}

			if (!last.IsGap)
			{
				last.End--;
				if (last.Start > transcript.TotalExonLength || last.End < 1) last.IsGap = true;
				result.AddLast(last);
			}

			return result;


		}

		/// <summary>
        /// assigns the backup cDNA positions (ignoring gaps)
        /// </summary>
        private static void AssignBackupCdnaPositions(LinkedList<Coordinate> coords, TranscriptAnnotation ta)
        {
            var nonGaps = new LinkedList<Coordinate>();
            foreach (var coord in coords) if (!coord.IsGap) nonGaps.AddLast(coord);

            if (nonGaps.Count == 0)
            {
                ta.BackupCdnaBegin = -1;
                ta.BackupCdnaEnd = -1;
                return;
            }

            ta.BackupCdnaBegin = nonGaps.First.Value.Start;
            ta.BackupCdnaEnd = nonGaps.Last.Value.End;
        }
    }
}
