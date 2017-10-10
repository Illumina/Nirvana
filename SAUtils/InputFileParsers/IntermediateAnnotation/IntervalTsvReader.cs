using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compression.Utilities;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;
using VariantAnnotation.SA;
using VariantAnnotation.Utilities;

namespace SAUtils.InputFileParsers.IntermediateAnnotation
{
    public sealed class IntervalTsvReader 
    {
        private readonly FileInfo _inputFileInfo;
        private string _name;
        private string _genomeAssembly;
        private string _version;
        private string _releaseDate;
        private string _description;
        private string _keyName;
        private ReportFor _reportFor;
        private readonly Dictionary<string, long> _refNameOffsets;


        private const int ChrIndex = 0;
        private const int StartIndex = 1;
        private const int EndIndex = 2;
        private const int JsonStringIndex = 3;

        private const int MinNoOfColumns = 4;

        public IntervalTsvReader(FileInfo inputFileInfo)
        {
            _inputFileInfo = inputFileInfo;

            using (var tsvIndex = new TsvIndex(new BinaryReader(FileUtilities.GetReadStream(inputFileInfo.FullName + ".tvi"))))
            {
                _refNameOffsets = tsvIndex.TagPositions;
            }


            using (var reader = GZipUtilities.GetAppropriateStreamReader(_inputFileInfo.FullName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Skip empty lines.
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (!line.StartsWith("#")) break;

                    ParseHeaderLine(line);
                }

            }
        }


        public IEnumerable<ISupplementaryInterval> GetAnnotationItems(string refName)
        {
            if (!_refNameOffsets.ContainsKey(refName)) yield break;

            var offset = _refNameOffsets[refName];

            using (var reader = GZipUtilities.GetAppropriateStreamReader(_inputFileInfo.FullName))
            {
                reader.BaseStream.Position = offset;
                string line;
                //getting to the chromosome
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                    // finding desired chromosome. We need this because the GetLocation for GZipStream may return a position a few lines before the start of the chromosome
                    if (line.StartsWith(refName + "\t")) break;
                }
                if (line == null) yield break;
                do
                {
                    //next chromosome
                    if (!line.StartsWith(refName + "\t")) yield break;

                    var annotationItem = ExtractItem(line);
                    if (annotationItem == null) continue;

                    yield return annotationItem;

                } while ((line = reader.ReadLine()) != null);
            }
        }


        private ISupplementaryInterval ExtractItem(string line)
        {
            var columns = line.Split('\t');
            if (columns.Length < MinNoOfColumns)
                throw new InvalidDataException("Line contains too few columns:\n" + line);

            var chromosome = columns[ChrIndex];
            var start = int.Parse(columns[StartIndex]);
            var end = int.Parse(columns[EndIndex]);
            var jsonString = columns[JsonStringIndex];

            return new SupplementaryInterval(_keyName, chromosome, start, end, jsonString, _reportFor);
        }

        public InterimIntervalHeader GetHeader()
        {
            if (string.IsNullOrEmpty(_name)           ||
                string.IsNullOrEmpty(_version)        ||
                string.IsNullOrEmpty(_releaseDate)    ||
                string.IsNullOrEmpty(_keyName)
                )
            {
                Console.WriteLine($"Insufficient version information for {_name}");
                return null;
            }

            return new InterimIntervalHeader(_name, _genomeAssembly, _version, _releaseDate, _description, _reportFor);
        }

        private void ParseHeaderLine(string line)
        {
            var words = line.Split('=');
            if (words.Length < 2) return;

            var key = words[0];
            var value = words[1];

            switch (key)
            {
                case "#name":
                    _name = value;
                    break;
                case "#assembly":
                    _genomeAssembly = value;
                    break;
                case "#version":
                    _version = value;
                    break;
                case "#releaseDate":
                    _releaseDate = value;
                    break;
                case "#description":
                    _description = value;
                    break;
                case "#keyName":
                    _keyName = value;
                    break;
                case "#reportFor":
                    _reportFor = GetReportFor(value);
                    break;
                case "#schemaVerstion":
                    var schemaVersion = int.Parse(value);
                    if (schemaVersion != JsonCommon.SupplementarySchemaVersion)
                        throw new InvalidDataException($"Expected Schema version:{JsonCommon.SupplementarySchemaVersion}, oberved: {value}");
                    break;
            }
        }

        private ReportFor GetReportFor(string value)
        {
            switch (value)
            {
                case "StructuralVariants":
                    return ReportFor.StructuralVariants;

                case "SmallVariants":
                    return ReportFor.SmallVariants;

                case "All":
                    return ReportFor.AllVariants;
            }

            return ReportFor.AllVariants;
        }

        
        public List<string> GetAllRefNames()
        {
            return _refNameOffsets.Keys.ToList();
        }
    }
}
