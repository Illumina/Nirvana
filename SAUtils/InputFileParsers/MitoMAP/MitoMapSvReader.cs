using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ErrorHandling.Exceptions;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.ClinVar;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Providers;

namespace SAUtils.InputFileParsers.MitoMAP
{
    public class MitoMapSvReader
    {
        private readonly FileInfo _mitoMapFileInfo;
        public readonly string DataType;
        private readonly ReferenceSequenceProvider _sequenceProvider;
        private readonly CircularGenomeModel _mitoGenomeModel;
        private readonly VariantAligner _variantAligner;


        private readonly HashSet<string> _mitoMapSvDataTypes = new HashSet<string>()
        {
            MitoMapDataTypes.MitoMapDeletionsSingle, 
            MitoMapDataTypes.MitoMapInsertionsSimple
        };

        public MitoMapSvReader(FileInfo mitoMapFileInfo, ReferenceSequenceProvider sequenceProvider)
        {
            _mitoMapFileInfo = mitoMapFileInfo;
            DataType = GetDataType();
            _sequenceProvider = sequenceProvider;
            _mitoGenomeModel = new CircularGenomeModel(sequenceProvider);
            _variantAligner = new VariantAligner(sequenceProvider.Sequence);
        }

        private string GetDataType()
        {
            var dataType = _mitoMapFileInfo.Name.Replace(".html", null);
            if (!_mitoMapSvDataTypes.Contains(dataType)) throw new InvalidFileFormatException($"Unexpected data file: {_mitoMapFileInfo.Name}");
            return dataType;
        }


        private IEnumerable<MitoMapItem> GetMitoMapItems()
        {
            bool isDataLine = false;
            using (var reader = new StreamReader(_mitoMapFileInfo.FullName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (!isDataLine)
                    {
                        if (line == "\"data\":[") isDataLine = true;
                        continue;
                    }
                    // last item
                    if (line.StartsWith("[") && line.EndsWith("]],")) isDataLine = false;

                    foreach (var supplementaryIntervalItem in ParseLine(line, DataType))
                    {
                        yield return supplementaryIntervalItem;
                    }
                }
            }
        }


        private List<MitoMapItem> ParseLine(string line, string dataType)
        {
            // line validation
            if (!(line.StartsWith("[") && line.EndsWith("],")))
                throw new InvalidFileFormatException($"Data line doesn't start with \"[\" or end with \"],\": {line}");
            var info = line.TrimEnd(',').TrimEnd(']').Trim('[', ']').Split("\",\"").Select(x => x.Trim('"')).ToList();
            if (dataType == MitoMapDataTypes.MitoMapInsertionsSimple)
                return ExtractSvItemFromSimpleInsertions(info);
            return ExtractSvItemFromDeletionsSingle(info);
        }

        private List<MitoMapItem> ExtractSvItemFromDeletionsSingle(List<string> info)
        {
            var junctions = info[0].Split(':').Select(int.Parse).ToList();
            var start = junctions[0] + 1;
            var end = junctions[1] - 1;
            if (end < start) Console.WriteLine($"Deletions with end position smaller than start position: start: {start}, end: {end}");
            var calculatedSize = end - start + 1;
            var size = int.Parse(info[1].Substring(1));
            if (calculatedSize != size) Console.WriteLine($"Incorrect size of deleted region: size of {start}-{end} should be {calculatedSize}, provided size is {size}");
            var refSequence = _sequenceProvider.Sequence.Substring(start - 1, size);
            var newStart = _variantAligner.LeftAlign(start, refSequence, "").Item1;
            if (start != newStart) Console.WriteLine($"Deletion of {calculatedSize} bps. Original start start position: {start}; new position after left-alignment {newStart}.");
            var mitoMapItem = new MitoMapItem(newStart, "", "", "", null, null, "", "", "", true, newStart + size - 1, VariantType.deletion);
            return new List<MitoMapItem> { mitoMapItem };

        }

        // extract large insertions from this file
        private List<MitoMapItem> ExtractSvItemFromSimpleInsertions(List<string> info)
        {
            var svItems = new List<MitoMapItem>();
            var altAlleleInfo = info[2];
            var dLoopPattern = new Regex(@"(?<start>^\d+)-(?<end>(\d+)) D-Loop region");
            var dLoopMatch = dLoopPattern.Match(altAlleleInfo);
            // not a large insertion
            if (!dLoopMatch.Success) return svItems;
            var genomeStart = MitoDLoop.Start + int.Parse(dLoopMatch.Groups["start"].Value) - 1;
            var genomeEnd = MitoDLoop.Start + int.Parse(dLoopMatch.Groups["end"].Value) - 1;
            foreach (var interval in _mitoGenomeModel.GetLinearIntervals(genomeStart, genomeEnd))
            {
                var mitoMapItem = new MitoMapItem(interval.Item1, "", "", "", null, null, "", "", "", true, interval.Item2, VariantType.duplication);
                svItems.Add(mitoMapItem);
            }
            return svItems;
        }
        public static IEnumerator<MitoMapItem> MergeAndSort(List<MitoMapSvReader> mitoMapSvReaders) => mitoMapSvReaders.SelectMany(x => x.GetMitoMapItems()).OrderBy(x => x.Start).GetEnumerator();
    }
}
