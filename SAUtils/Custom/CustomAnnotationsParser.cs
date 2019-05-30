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
    public sealed class CustomAnnotationsParser : IDisposable
    {
        private readonly StreamReader _reader;
        public ISequenceProvider SequenceProvider { get; set; }
        public string JsonTag;
        public GenomeAssembly Assembly;
        public bool MatchByAllele;
        public bool IsArray;
        private string[] _tags;
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


        internal CustomAnnotationsParser(StreamReader streamReader, ISequenceProvider sequenceProvider)
        {
            _reader = streamReader;
            SequenceProvider = sequenceProvider;
            _intervals = new List<CustomInterval>();
        }

        public static CustomAnnotationsParser Create(StreamReader streamReader, ISequenceProvider sequenceProvider = null)
        {
            var parser = new CustomAnnotationsParser(streamReader, sequenceProvider);

            parser.ParseHeaderLines();
            parser.InitiateSchema();
            parser.AddPredefinedTypeAnnotation();
            parser.AddHeaderAnnotation();

            return parser;
        }

        internal void ParseHeaderLines()
        {
            ParseTitle();
            ParseGenomeAssembly();
            ParseMatchVariantsBy();
            ParseTags();
            ParseCategories();
            ParseDescriptions();
            ParseTypes();
        }

        internal void ParseTitle()
        {
            string line = ReadlineAndCheckPrefix("#title", "first");
            string firstCol = line.OptimizedSplit('\t')[0];
            (_, string jsonTag) = firstCol.OptimizedKeyValue();

            if (jsonTag == null)
                throw new UserErrorException("Please provide the title in the format: #title=titleValue.");

            if (CheckJsonTagConflict(jsonTag))
                throw new UserErrorException($"{jsonTag} is a reserved supplementary annotation tag in Nirvana. Please use a different value.");
            JsonTag = jsonTag;
        }

        internal void ParseGenomeAssembly()
        {
            string line = ReadlineAndCheckPrefix("#assembly", "second");
            string firstCol = line.OptimizedSplit('\t')[0];
            (_, string assemblyString) = firstCol.OptimizedKeyValue();

            if (assemblyString == null)
                throw new UserErrorException("Please provide the genome assembly in the format: #assembly=genomeAssembly.");

            Assembly = GenomeAssemblyHelper.Convert(assemblyString);
            if (!_allowedGenomeAssemblies.Contains(Assembly))
                throw new UserErrorException("Only GRCh37 and GRCh38 are accepted for genome assembly.");
        }

        private void ParseMatchVariantsBy()
        {
            string line = ReadlineAndCheckPrefix("#matchVariantsBy", "third");
            string firstCol = line.OptimizedSplit('\t')[0];
            (_, string matchBy) = firstCol.OptimizedKeyValue();

            if (matchBy== null)
                throw new UserErrorException("Please provide the genome assembly in the format: #matchVariantsBy=allele.");

            if (matchBy == "allele")
            {
                MatchByAllele = true;
                IsArray = false;
                _primaryType=SaJsonValueType.Object;
            }
            if (matchBy == "position")
            {
                _primaryType = SaJsonValueType.ObjectArray;
                MatchByAllele = false;
                IsArray = true;
            }

            if(! (IsArray^MatchByAllele))
                throw new UserErrorException($"matchVariantsBy tag has to be either \'allele\' or \'position\'");
        }

        internal void ParseTags()
        {
            var line = ReadlineAndCheckPrefix("#CHROM", "fourth");

            _tags = line.OptimizedSplit('\t');
            if (_tags.Length < 4)
                throw new UserErrorException("At least 4 columns required. Please note that the columns should be separated by tab.");

            CheckPosAndRefColumns();
            CheckAltAndEndColumns();

            for (int i = _numRequiredColumns; i < _tags.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(_tags[i]))
                    throw new UserErrorException($"Please provide a name for column {i + 1} at the third row.");

                JsonKeys.Add(_tags[i]);
                IntervalJsonKeys.Add(_tags[i]);
            }
        }

        private void CheckPosAndRefColumns()
        {
            if (_tags[1] != "POS" || _tags[2] != "REF")
                throw new UserErrorException("The 2nd and 3rd columns must be POS and REF, respectively.");
        }

        private void CheckAltAndEndColumns()
        {
            _numRequiredColumns = 4;

            switch (_tags[3])
            {
                case "ALT":
                    {
                        _altColumnIndex = 3;

                        if (_tags.Length > 4 && _tags[4] == "END")
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

            _numAnnotationColumns = _tags.Length - _numRequiredColumns;
            _annotationValidators = Enumerable.Repeat<Action<string, string>>(
                    (a, b) => { }, _numAnnotationColumns).ToArray();
        }

        private void ParseCategories()
        {
            var line = ReadlineAndCheckPrefix("#categories", "fifth");
            var splits = line.OptimizedSplit('\t');
            if (splits.Length != _tags.Length) throw new UserErrorException("#categories row must have the same number of columns as the #CHROM row.");

            Categories = new CustomAnnotationCategories[_numAnnotationColumns];
            for (var i = 0; i < _numAnnotationColumns; i++)
            {
                switch (splits[i + _numRequiredColumns].ToLower())
                {
                    case "allelecount":
                        Categories[i] = CustomAnnotationCategories.AlleleCount;
                        break;
                    case "allelenumber":
                        Categories[i] = CustomAnnotationCategories.AlleleNumber;
                        break;
                    case "allelefrequency":
                        Categories[i] = CustomAnnotationCategories.AlleleFrequency;
                        break;
                    case "prediction":
                        Categories[i] = CustomAnnotationCategories.Prediction;
                        _annotationValidators[i] = AllowedValues.ValidatePredictionValue;
                        break;
                    case "filter":
                        Categories[i] = CustomAnnotationCategories.Filter;
                        _annotationValidators[i] = AllowedValues.ValidateFilterValue;
                        break;
                    default:
                        Categories[i] = CustomAnnotationCategories.Unknown;
                        break;
                }
            }
        }

        private void ParseDescriptions()
        {
            var line = ReadlineAndCheckPrefix("#descriptions", "sixth");
            var splits = line.OptimizedSplit('\t');
            if (splits.Length != _tags.Length) throw new UserErrorException("#descriptions row must have the same number of columns as the #CHROM row");

            Descriptions = new string[_numAnnotationColumns];
            for (int i = 0; i < _numAnnotationColumns; i++)
            {
                if (splits[i + _numRequiredColumns] == ".") Descriptions[i] = null;
                else Descriptions[i] = splits[i + _numRequiredColumns];
            }
        }

        internal void ParseTypes()
        {
            var line = ReadlineAndCheckPrefix("#type", "seventh");
            var splits = line.OptimizedSplit('\t');
            if (splits.Length != _tags.Length) throw new UserErrorException("#types row must have the same number of columns as the #CHROM row");

            ValueTypes = new SaJsonValueType[_numAnnotationColumns];
            for (int i = 0; i < _numAnnotationColumns; i++)
            {
                switch (splits[i + _numRequiredColumns].ToLower())
                {
                    case "bool":
                        ValueTypes[i] = SaJsonValueType.Bool;
                        break;
                    case "string":
                        ValueTypes[i] = SaJsonValueType.String;
                        break;
                    case "number":
                        ValueTypes[i] = SaJsonValueType.Number;
                        break;
                    default:
                        throw new UserErrorException("Invalid value for type column. Valid values are bool, string, number.");
                }
            }
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
            foreach (var (jsonKey, valueType) in _predefinedTypeAnnotation)
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

                JsonSchema?.AddAnnotation(_tags[i + _numRequiredColumns], annotation);
                IntervalJsonSchema?.AddAnnotation(_tags[i + _numRequiredColumns], annotation);
            }
        }

        internal CustomItem ExtractItems(string line)
        {
            var splits = line.OptimizedSplit('\t');
            if (splits.Length != _tags.Length)
                throw new UserErrorException($"Column number mismatch!! Header has {_tags.Length} columns but {line} contains {splits.Length}");

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

        internal string ReadlineAndCheckPrefix(string prefix, string rowNumber)
        {
            string line = _reader.ReadLine();
            if (line != null && !line.StartsWith(prefix))
                throw new UserErrorException($"The TSV file is required to start with {prefix} in the {rowNumber} row.");

            return line;
        }

        private static bool CheckJsonTagConflict(string value)
        {
            return value.Equals(SaCommon.DbsnpTag)
                   || value.Equals(SaCommon.GlobalAlleleTag)
                   || value.Equals(SaCommon.AncestralAlleleTag)
                   || value.Equals(SaCommon.ClinGenTag)
                   || value.Equals(SaCommon.ClinvarTag)
                   || value.Equals(SaCommon.CosmicTag)
                   || value.Equals(SaCommon.CosmicCnvTag)
                   || value.Equals(SaCommon.DgvTag)
                   || value.Equals(SaCommon.ExacScoreTag)
                   || value.Equals(SaCommon.GnomadTag)
                   || value.Equals(SaCommon.GnomadExomeTag)
                   || value.Equals(SaCommon.MitoMapTag)
                   || value.Equals(SaCommon.OmimTag)
                   || value.Equals(SaCommon.OneKgenTag)
                   || value.Equals(SaCommon.OnekSvTag)
                   || value.Equals(SaCommon.PhylopTag)
                   || value.Equals(SaCommon.RefMinorTag)
                   || value.Equals(SaCommon.TopMedTag);
        }

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