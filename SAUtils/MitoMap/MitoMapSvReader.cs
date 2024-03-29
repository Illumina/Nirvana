﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ErrorHandling.Exceptions;
using Genome;
using IO;
using OptimizedCore;
using SAUtils.InputFileParsers.ClinVar;
using VariantAnnotation.Interface.Providers;
using Variants;

namespace SAUtils.MitoMap
{
    public sealed class MitoMapSvReader
    {
        private readonly FileInfo _mitoMapFileInfo;
        private readonly string _dataType;
        private readonly ISequenceProvider _sequenceProvider;
        private readonly VariantAligner _variantAligner;
        private readonly Chromosome _chromosome;

        private readonly HashSet<string> _mitoMapSvDataTypes = new HashSet<string>
        {
            MitoMapDataTypes.MitoMapDeletionsSingle,
            MitoMapDataTypes.MitoMapInsertionsSimple
        };

        public MitoMapSvReader(FileInfo mitoMapFileInfo, ISequenceProvider sequenceProvider)
        {
            _mitoMapFileInfo = mitoMapFileInfo;
            _dataType = GetDataType();
            _sequenceProvider = sequenceProvider;
            _chromosome = sequenceProvider?.RefNameToChromosome["chrM"] ;
            _variantAligner = new VariantAligner(sequenceProvider?.Sequence);
        }

        private string GetDataType()
        {
            string dataType = _mitoMapFileInfo.Name.Replace(".html", null, StringComparison.Ordinal);
            if (!_mitoMapSvDataTypes.Contains(dataType)) throw new InvalidFileFormatException($"Unexpected data file: {_mitoMapFileInfo.Name}");
            return dataType;
        }


        private IEnumerable<MitoMapSvItem> GetMitoMapSvItems()
        {
            bool isDataLine = false;
            using (var reader = FileUtilities.GetStreamReader(FileUtilities.GetReadStream(_mitoMapFileInfo.FullName)))
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
                    if (line.OptimizedStartsWith('[') && line.EndsWith("]],", StringComparison.Ordinal)) isDataLine = false;

                    foreach (var supplementaryIntervalItem in ParseLine(line))
                    {
                        yield return supplementaryIntervalItem;
                    }
                }
            }
        }

        internal List<MitoMapSvItem> ParseLine(string line)
        {
            // line validation
            if (!(line.OptimizedStartsWith('[') && line.EndsWith("],", StringComparison.Ordinal)))
                throw new InvalidFileFormatException($"Data line doesn't start with \"[\" or end with \"],\": {line}");
            var info = line.TrimEnd(',').TrimEnd(']').Trim('[', ']').Split("\",\"").Select(x => x.Trim('"')).ToList();
            return _dataType == MitoMapDataTypes.MitoMapInsertionsSimple ? ExtractSvItemFromSimpleInsertions(info) : ExtractSvItemFromDeletionsSingle(info);
        }

        private List<MitoMapSvItem> ExtractSvItemFromDeletionsSingle(List<string> info)
        {
            var junctions = info[0].OptimizedSplit(':').Select(int.Parse).ToList();
            var start = junctions[0] + 1; 
            var end = junctions[1] - 1;
            if (end < start)
                throw new ArgumentOutOfRangeException($"Deletions with end position smaller than start position: start: {start}, end: {end}");
            var calculatedSize = end - start + 1;
            var size = int.Parse(info[1].Substring(1));
            if (size <= MitomapParsingParameters.LargeDeletionCutoff) return new List<MitoMapSvItem>();
            if (calculatedSize != size) Console.WriteLine($"Incorrect size of deleted region: size of {start}-{end} should be {calculatedSize}, provided size is {size}. Provided size is used.");
            var refSequence = _sequenceProvider.Sequence.Substring(start - 1, size);
            var newStart = _variantAligner.LeftAlign(start, refSequence, "").Item1;
            if (start != newStart) Console.WriteLine($"Deletion of {size} bps. Original start start position: {start}; new position after left-alignment {newStart}.");
            var mitoMapSvItem = new MitoMapSvItem(_chromosome, newStart, newStart + size - 1, VariantType.deletion);
            return new List<MitoMapSvItem> { mitoMapSvItem };
        }

        // extract large insertions from this file
        private List<MitoMapSvItem> ExtractSvItemFromSimpleInsertions(IReadOnlyList<string> info)
        {
            var mitoMapSvItems = new List<MitoMapSvItem>();
            var altAlleleInfo = info[2];
            var dLoopPattern = new Regex(@"(?<start>^\d+)-(?<end>(\d+)) D-Loop region");
            var dLoopMatch = dLoopPattern.Match(altAlleleInfo);
            // not a large insertion
            if (!dLoopMatch.Success) return mitoMapSvItems;
            var genomeStart = MitoDLoop.Start + int.Parse(dLoopMatch.Groups["start"].Value) - 1;
            var genomeEnd = MitoDLoop.Start + int.Parse(dLoopMatch.Groups["end"].Value) - 1;
            if (genomeEnd < genomeStart)
                throw new ArgumentOutOfRangeException($"Duplication with end position smaller than start position: start: {genomeStart}, end: {genomeEnd}");
            var size = genomeEnd - genomeStart + 1;
            var refSequence = _sequenceProvider.Sequence.Substring(genomeStart - 1, size);
            var leftAlignResults = _variantAligner.LeftAlign(genomeStart, refSequence, refSequence + refSequence); // duplication
            var newStart = leftAlignResults.Item1;
            if (genomeStart != newStart) Console.WriteLine($"Duplication of {size} bps. Original start start position: {genomeStart}; new position after left-alignment {newStart}.");
            var mitoMapSvItem = new MitoMapSvItem(_chromosome, newStart, newStart + size - 1, VariantType.duplication);
            mitoMapSvItems.Add(mitoMapSvItem);
            return mitoMapSvItems;
        }
        
        public static IEnumerable<MitoMapSvItem> GetSortedItems(IEnumerable<MitoMapSvReader> mitoMapSvReaders) => mitoMapSvReaders.SelectMany(x => x.GetMitoMapSvItems()).OrderBy(x => x.Start);
    }
}
