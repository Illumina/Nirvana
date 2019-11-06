using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ErrorHandling.Exceptions;
using Genome;
using OptimizedCore;
using SAUtils.DataStructures;
using SAUtils.Schema;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.SA;
using Variants;

namespace SAUtils.Custom
{
    public sealed class VariantAnnotationsParser : IDisposable
    {
        private readonly StreamReader _reader;
        public ISequenceProvider SequenceProvider;
        public string JsonTag;
        public GenomeAssembly Assembly;
        public bool MatchByAllele;
        public bool IsArray;
        public string[] Tags;
        internal CustomAnnotationCategories[] Categories;
        internal string[] Descriptions;
        internal SaJsonValueType[] ValueTypes;

        private int _numRequiredColumns;
        private int _numAnnotationColumns;
        private int _altColumnIndex = -1;
        private int _endColumnIndex = -1;
        private readonly HashSet<GenomeAssembly> _allowedGenomeAssemblies = new HashSet<GenomeAssembly> { GenomeAssembly.GRCh37, GenomeAssembly.GRCh38 };
        private readonly List<CustomInterval> _intervals;
        private (IChromosome Chromesome, int Position) _previousPosition = (null, 0);
        private Action<string, string>[] _annotationValidators;

        private SaJsonValueType _primaryType;
        private readonly Dictionary<string, SaJsonValueType> _predefinedTypeAnnotation = new Dictionary<string, SaJsonValueType>
        {
            {"refAllele", SaJsonValueType.String},
            {"altAllele", SaJsonValueType.String},
            {"start", SaJsonValueType.Number},
            {"end", SaJsonValueType.Number}
        };

        internal readonly List<string> JsonKeys = new List<string> { "refAllele", "altAllele" };
        internal readonly List<string> IntervalJsonKeys = new List<string> { "start", "end" };

        public SaJsonSchema JsonSchema;
        public SaJsonSchema IntervalJsonSchema;


        internal VariantAnnotationsParser(StreamReader streamReader, ISequenceProvider sequenceProvider)
        {
            _reader = streamReader;
            SequenceProvider = sequenceProvider;
            _intervals = new List<CustomInterval>();
        }

        public static VariantAnnotationsParser Create(StreamReader streamReader, ISequenceProvider sequenceProvider = null)
        {
            var parser = new VariantAnnotationsParser(streamReader, sequenceProvider);

            parser.ParseHeaderLines();
            parser.InitiateSchema();
            parser.AddPredefinedTypeAnnotation();
            parser.AddHeaderAnnotation();

            return parser;
        }

        internal void ParseHeaderLines()
        {
            JsonTag = ParserUtilities.ParseTitle(_reader.ReadLine());
            Assembly = ParserUtilities.ParseGenomeAssembly(_reader.ReadLine(), _allowedGenomeAssemblies);
            (MatchByAllele, IsArray, _primaryType) = ParserUtilities.ParseMatchVariantsBy(_reader.ReadLine());
            Tags = ParserUtilities.ParseTags(_reader.ReadLine(), "#CHROM", _numRequiredColumns, "forth");
            CheckTagsAndSetJsonKeys();
            Categories = ParserUtilities.ParseCategories(_reader.ReadLine(), _numRequiredColumns, _numAnnotationColumns, _annotationValidators, "fifth");
            Descriptions = ParserUtilities.ParseDescriptions(_reader.ReadLine(), _numRequiredColumns, _numAnnotationColumns, "sixth");
            ValueTypes = ParserUtilities.ParseTypes(_reader.ReadLine(), _numRequiredColumns, _numAnnotationColumns, "seventh");
        }

        private void CheckTagsAndSetJsonKeys()
        {
            CheckPosAndRefColumns();
            CheckAltAndEndColumns();

            for (int i = _numRequiredColumns; i < Tags.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(Tags[i]))
                    throw new UserErrorException($"Please provide a name for column {i + 1} at the forth row.");

                JsonKeys.Add(Tags[i]);
                IntervalJsonKeys.Add(Tags[i]);
            }
        }

        internal void CheckPosAndRefColumns()
        {
            if (Tags[1] != "POS" || Tags[2] != "REF")
                throw new UserErrorException("The 2nd and 3rd columns must be POS and REF, respectively.");
        }

        internal void CheckAltAndEndColumns()
        {
            _numRequiredColumns = 4;

            switch (Tags[3])
            {
                case "ALT":
                    {
                        _altColumnIndex = 3;

                        if (Tags.Length > 4 && Tags[4] == "END")
                        {
                            _endColumnIndex = 4;
                            _numRequiredColumns = 5;
                        }

                        break;
                    }
                case "END":
                    _endColumnIndex = 3;
                    break;
                default:
                    throw new UserErrorException("Please provide at least one of the ALT and END columns.The END column should come after the ALT column if both are present.");
            }

            _numAnnotationColumns = Tags.Length - _numRequiredColumns;
            _annotationValidators = Enumerable.Repeat<Action<string, string>>((a, b) => { }, _numAnnotationColumns).ToArray();
        }

        public IEnumerable<CustomItem> GetItems()
        {
            if (SequenceProvider == null)
            {
                throw new Exception("Sequence provider is null.");
            }
            using (_reader)
            {
                string line;
                while ((line = _reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var item = ExtractItems(line);
                    if (item == null) continue;
                    yield return item;
                }
            }
        }

        private void InitiateSchema()
        {
            if (_altColumnIndex != -1) JsonSchema = SaJsonSchema.Create(new StringBuilder(), JsonTag, _primaryType, JsonKeys);
            if (_endColumnIndex != -1) IntervalJsonSchema = SaJsonSchema.Create(new StringBuilder(), JsonTag, SaJsonValueType.ObjectArray, IntervalJsonKeys);
        }

        private void AddPredefinedTypeAnnotation()
        {
            foreach ((string jsonKey, var valueType) in _predefinedTypeAnnotation)
            {
                JsonSchema?.AddAnnotation(jsonKey, SaJsonKeyAnnotation.CreateFromProperties(valueType, 0, null));
                IntervalJsonSchema?.AddAnnotation(jsonKey, SaJsonKeyAnnotation.CreateFromProperties(valueType, 0, null));
            }
        }

        private void AddHeaderAnnotation()
        {
            for (var i = 0; i < _numAnnotationColumns; i++)
            {
                var annotation = SaJsonKeyAnnotation.CreateFromProperties(ValueTypes[i], Categories[i], Descriptions[i]);

                JsonSchema?.AddAnnotation(Tags[i + _numRequiredColumns], annotation);
                IntervalJsonSchema?.AddAnnotation(Tags[i + _numRequiredColumns], annotation);
            }
        }

        internal CustomItem ExtractItems(string line)
        {
            var splits = line.OptimizedSplit('\t');
            if (splits.Length != Tags.Length)
                throw new UserErrorException($"Column number mismatch!! Header has {Tags.Length} columns but {line} contains {splits.Length}");

            string chromosome = splits[0];

            if (!SequenceProvider.RefNameToChromosome.TryGetValue(chromosome, out var chrom))
            {
                Console.WriteLine($"Annotation on {chromosome} is skipped.");
                return null;
            }

            SequenceProvider.LoadChromosome(chrom);

            if (!int.TryParse(splits[1], out var position))
                throw new UserErrorException($"POS is not an int number at: {line}.");

            CheckAnnotationSorted(chrom, position, line);

            string refAllele = splits[2].ToUpper();

            var annotationValues = new string[_numAnnotationColumns];
            for (var i = 0; i < _numAnnotationColumns; i++)
            {
                annotationValues[i] = splits[i + _numRequiredColumns];
                _annotationValidators[i](annotationValues[i], line);
            }

            if (IsInterval(splits))
            {
                var jsonStringValues = new List<string> { splits[1], splits[_endColumnIndex] };

                if (!int.TryParse(splits[_endColumnIndex], out var end))
                    throw new UserErrorException($"END is not an integer.\nInput line: {line}.");

                jsonStringValues.AddRange(annotationValues);
                _intervals.Add(new CustomInterval(chrom, position, end, jsonStringValues.Select(x => new[] { x }).ToList(), IntervalJsonSchema, line));
                return null;
            }

            string altAllele = splits[_altColumnIndex];
            if (!IsValidNucleotideSequence(altAllele))
                throw new UserErrorException($"Invalid nucleotides in ALT column: {altAllele}.\nInput line: {line}");

            (position, refAllele, altAllele) = VariantUtils.TrimAndLeftAlign(position, refAllele, altAllele, SequenceProvider.Sequence);
            return new CustomItem(chrom, position, refAllele, altAllele, annotationValues.Select(x => new[] { x }).ToArray(), JsonSchema, line);
        }

        private bool IsInterval(string[] splits) => _endColumnIndex != -1 && !AllowedValues.IsEmptyValue(splits[_endColumnIndex]);

        private void CheckAnnotationSorted(IChromosome chrom, int position, string line)
        {
            if (chrom != _previousPosition.Chromesome)
            {
                _previousPosition = (chrom, position);
            }
            else
            {
                if (position < _previousPosition.Position)
                    throw new UserErrorException($"Annotation is not sorted at {line}");
                _previousPosition.Position = position;
            }
        }

        public List<CustomInterval> GetCustomIntervals() => _intervals.Count > 0 ? _intervals : null;

        internal static bool IsValidNucleotideSequence(string sequence)
        {
            var validNucleotides = new[] { 'a', 'c', 'g', 't', 'n' };
            foreach (char nucleotide in sequence.ToLower())
            {
                if (!validNucleotides.Contains(nucleotide)) return false;
            }

            return true;
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}