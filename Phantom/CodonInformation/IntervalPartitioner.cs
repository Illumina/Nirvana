using System.Collections.Generic;
using System.Linq;
using Intervals;

namespace Phantom.CodonInformation
{
    public static class IntervalPartitioner
    {
        public static List<IInterval>[] GetCommonIntervals(TranscriptIntervalsInGene transcript)
        {
            int numTranscripts = transcript.NumTranscripts;
            var transcriptIntervals = new List<IInterval>[numTranscripts];
            for (int i = 0; i < numTranscripts; i++) transcriptIntervals[i] = new List<IInterval>();
            var inCodingRegion = new Dictionary<int, bool>();

            var boundaryInfo = GetBoundaryInfo(transcript.Intervals);
            var sortedBoundaries = boundaryInfo.OrderBy(x => x.Key);

            int startPosition = -1;
            foreach (var boundary in sortedBoundaries)
            {
                startPosition = ProcessBoundary(boundary, startPosition, inCodingRegion, transcriptIntervals);
            }
            return transcriptIntervals;
        }

        private static Dictionary<int, BoundaryStatus> GetBoundaryInfo(IInterval[][] intervalLists)
        {
            var boundaryInfo = new Dictionary<int, BoundaryStatus>();
            for (int i = 0; i < intervalLists.Length; i++)
            {
                foreach (var interval in intervalLists[i])
                {
                    if (interval.Start != interval.End)
                    {
                        SetOrUpdateBoundaryInfo(boundaryInfo, "Starts", interval.Start, i);
                        SetOrUpdateBoundaryInfo(boundaryInfo, "Ends", interval.End, i);
                    }
                    else
                    {
                        SetOrUpdateBoundaryInfo(boundaryInfo, "SingleBaseBlocks", interval.Start, i);
                    }
                }
            }
            return boundaryInfo;
        }

        private static void SetOrUpdateBoundaryInfo(IDictionary<int, BoundaryStatus> boundaryInfo, string propertyName, int position, int i)
        {
            var property = typeof(BoundaryStatus).GetProperty(propertyName);
            List<int> propertyValue;
            if (boundaryInfo.TryGetValue(position, out var overlappingTranscripts))
            {
                propertyValue = (List<int>)property.GetValue(overlappingTranscripts);

            }
            else
            {
                var boundaryStatus = new BoundaryStatus();
                boundaryInfo.Add(position, boundaryStatus);
                propertyValue = (List<int>)property.GetValue(boundaryStatus);
            }
            propertyValue.Add(i);
        }


        private static int ProcessBoundary(KeyValuePair<int, BoundaryStatus> boundary, int startPosition,
            IDictionary<int, bool> inCodingRegion, IReadOnlyList<List<IInterval>> transcriptIntervals)
        {

            int thisPosition = boundary.Key;

            bool hasStarts = boundary.Value.Starts.Count > 0;
            bool hasEnds = boundary.Value.Ends.Count > 0;
            bool hasSingleBases = boundary.Value.SingleBaseBlocks.Count > 0;
            
            //first boundary and no single base block
            if (startPosition == -1 && !hasSingleBases)
            {
                boundary.Value.Starts.ForEach(x => inCodingRegion[x] = true);
                return thisPosition;
            }

            boundary.Value.Ends.ForEach(x => inCodingRegion[x] = false);

            // transcripts with exons overlapped with this position
            var overlappingTranscripts = new List<int>();
            foreach (var (transcriptId, isCodingRegion) in inCodingRegion)
            {
                if (isCodingRegion) overlappingTranscripts.Add(transcriptId);
            }

            boundary.Value.Starts.ForEach(x => inCodingRegion[x] = true);


            // only ends and possible overlapping transcripts
            if (!hasStarts && hasEnds && !hasSingleBases)
            {
                // assign overlapping and ending transcripts to the interval ending at this position
                var commonIntervalEndingThisPosition = new Interval(startPosition, thisPosition);
                overlappingTranscripts.ForEach(x => transcriptIntervals[x].Add(commonIntervalEndingThisPosition));
                boundary.Value.Ends.ForEach(x => transcriptIntervals[x].Add(commonIntervalEndingThisPosition));
                return thisPosition + 1;
            }

            var commonIntervalBefore = new Interval(startPosition, thisPosition - 1);
            // only starts and possible overlapping transcripts
            if (hasStarts && !hasEnds && !hasSingleBases)
            {
                // assign overlapping transcripts to the interval before this position
                overlappingTranscripts.ForEach(x => transcriptIntervals[x].Add(commonIntervalBefore));
                return thisPosition;
            }

            // have both starts and ends, and/or singlebases
            // assign overlapping and ending transcripts to the interval before this position 
            overlappingTranscripts.ForEach(x => transcriptIntervals[x].Add(commonIntervalBefore));
            boundary.Value.Ends.ForEach(x => transcriptIntervals[x].Add(commonIntervalBefore));

            // assign starting, ending, overlapping and single-base-block transcripts to the single base interval 
            var commonIntervalThisPosition = new Interval(thisPosition, thisPosition);
            overlappingTranscripts.ForEach(x => transcriptIntervals[x].Add(commonIntervalThisPosition));
            boundary.Value.Starts.ForEach(x => transcriptIntervals[x].Add(commonIntervalThisPosition));
            boundary.Value.Ends.ForEach(x => transcriptIntervals[x].Add(commonIntervalThisPosition));
            boundary.Value.SingleBaseBlocks.ForEach(x => transcriptIntervals[x].Add(commonIntervalThisPosition));

            return thisPosition + 1;
        }
    }
}
