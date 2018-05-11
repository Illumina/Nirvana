using System;
using System.Collections.Generic;
using System.Linq;
using Compression.Utilities;
using Intervals;

namespace SAUtils.ProcessSpliceNetTsv
{
    public static class PredictionFilter
    {
        private const int GffChrColumn = 0;
        private const int GffFeatureColumn = 2;
        private const int GffStartColumn = 3;
        private const int GffEndColumn = 4;
        private const int NumChrs = 25;
        private const int PredChrColumn = 0;
        private const int PredPosColumn = 1;
        private static readonly int[] PredScoreColumns = { 6, 8, 10, 12 };
        private static readonly double _freqCutoff = 0.05;
        private static readonly int _intronBoundaryDistanceCutoff = 15;

        public static void Filter(string intputTsv, string gffFile1, string gffFile2, string outputTsv)
        {
            var intronFlankingRegions = GetIntronFlankingRegions(gffFile1, gffFile2);

            using (var resultsReader = GZipUtilities.GetAppropriateStreamReader(intputTsv))
            using (var resultsWriter = GZipUtilities.GetStreamWriter(outputTsv))
            {
                long lineCount = 0;
                string line;
                while ((line = resultsReader.ReadLine()) != null)
                {
                    var info = line.TrimEnd().Split('\t');
                    ushort chrIndex = GetChrIndex(info[PredChrColumn]);
                    int pos = int.Parse(info[PredPosColumn]);
                    if (intronFlankingRegions.OverlapsAny(chrIndex, pos, pos) ||
                        AnyScorePassTheCutoff(info, PredScoreColumns, _freqCutoff))
                    {
                        resultsWriter.WriteLine(line);
                    }
                    lineCount++;
                    if (lineCount % 1_000_000 == 0)
                    {
                        Console.WriteLine($"Processed {lineCount} lines. Current position: {info[PredChrColumn]}:{info[PredPosColumn]}");
                    }
                }
            }
        }

        private static bool AnyScorePassTheCutoff(string[] columns, int[] scoreColumnIndices, double scoreCutoff)
        {
            foreach (int columnIndex in scoreColumnIndices)
            {
                if (double.Parse(columns[columnIndex]) >= scoreCutoff) return true;
            }
            return false;
        }

        private static IntervalForest<byte> GetIntronFlankingRegions(string gffFile1, string gffFile2)
        {
            var flankingRegions = new IntervalArray<byte>[NumChrs];
            var flankingRegionStarts1 = GetIntronFlankingRegionStarts(gffFile1);
            var flankingRegionStarts2 = GetIntronFlankingRegionStarts(gffFile2);
            for (var i = 0; i < NumChrs; i++)
            {
                var allStartsThisChr = new HashSet<int>(flankingRegionStarts1[i]);
                allStartsThisChr.UnionWith(flankingRegionStarts2[i]);
                var intervals = GetIntervals(allStartsThisChr, _intronBoundaryDistanceCutoff * 2);
                flankingRegions[i] = new IntervalArray<byte>(intervals.ToArray());
            }
            return new IntervalForest<byte>(flankingRegions);
        }

        private static IEnumerable<Interval<byte>> GetIntervals(IEnumerable<int> starts, int size) => starts.Select(x => new Interval<byte>(x, x + size - 1, 0));

        private static HashSet<int>[] GetIntronFlankingRegionStarts(string gffFile)
        {

            var flankingRegionStarts = new HashSet<int>[NumChrs];
            for (var i = 0; i < NumChrs; i++) flankingRegionStarts[i] = new HashSet<int>();
            using (var gffReader = GZipUtilities.GetAppropriateStreamReader(gffFile))
            {
                string line;
                var previousChrIndex = ushort.MaxValue;
                var exonBoundaries = new List<Interval>();
                var flankingRegionStartsthisChr = new HashSet<int>();
                while ((line = gffReader.ReadLine()) != null)
                {
                    var info = line.Split('\t');
                    if (info[GffFeatureColumn] == "gene")
                    {
                        ushort chrIndex = GetChrIndex(info[GffChrColumn]);
                        if (previousChrIndex != ushort.MaxValue && chrIndex != previousChrIndex)
                        {
                            ProcessBufferedBoundaries(exonBoundaries, flankingRegionStartsthisChr);
                            flankingRegionStarts[previousChrIndex] = flankingRegionStartsthisChr;
                            flankingRegionStartsthisChr = new HashSet<int>();
                        }
                        previousChrIndex = chrIndex;
                    }
                    else if (info[GffFeatureColumn] == "transcript")
                    {
                        ProcessBufferedBoundaries(exonBoundaries, flankingRegionStartsthisChr);
                        exonBoundaries = new List<Interval>();
                    }
                    else if (info[GffFeatureColumn] == "exon")
                    {
                        int start = int.Parse(info[GffStartColumn]);
                        int end = int.Parse(info[GffEndColumn]);
                        exonBoundaries.Add(new Interval(start, end));
                    }
                }
                if (previousChrIndex != ushort.MaxValue)
                {
                    ProcessBufferedBoundaries(exonBoundaries, flankingRegionStartsthisChr);
                    flankingRegionStarts[previousChrIndex] = flankingRegionStartsthisChr;
                }
            }

            return flankingRegionStarts;
        }

        private static void ProcessBufferedBoundaries(List<Interval> exonBoundaries, HashSet<int> flankingRegionStartsthisChr)
        {
            for (var i = 1; i < exonBoundaries.Count; i++)
            {
                // Donor site for intron i
                flankingRegionStartsthisChr.Add(exonBoundaries[i - 1].End - _intronBoundaryDistanceCutoff + 1);
                // Acceptor site for intron i
                flankingRegionStartsthisChr.Add(exonBoundaries[i].Start - _intronBoundaryDistanceCutoff);
            }
        }

        private static ushort GetChrIndex(string chrName)
        {
            if (chrName.StartsWith("chr")) chrName = chrName.Substring(3);
            if (ushort.TryParse(chrName, out ushort chrNum))
            {
                return (ushort)(chrNum - 1);
            }
            switch (chrName)
            {
                case "X":
                    return 22;
                case "Y":
                    return 23;
                case "M":
                case "MT":
                    return 24;
                default:
                    return ushort.MaxValue;
            }
        }
    }
}