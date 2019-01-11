using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ErrorHandling.Exceptions;
using Genome;
using OptimizedCore;
using SAUtils.DataStructures;
using VariantAnnotation.SA;

namespace SAUtils.Custom
{
    public sealed class CustomAnnotationsParser:IDisposable
    {
        private readonly StreamReader _reader;
        private readonly IDictionary<string, IChromosome> _refChromDict;
        public string JsonTag;
        public GenomeAssembly Assembly;
        private string[] _tags;
        internal CustomAnnotationCategories[] Categories;
        internal string[] Descriptions;
        internal CustomAnnotationType[] Types;

        private int _numRequiredColumns;
        private int _numAnnotationColumns;
        private int _altColumnIndex = -1;
        private int _endColumnIndex = -1;
        private readonly HashSet<GenomeAssembly> _allowedGenomeAssemblies = new HashSet<GenomeAssembly>{GenomeAssembly.GRCh37, GenomeAssembly.GRCh38};
        private readonly List<CustomInterval> _intervals;

        private const string DataType = "array";
        private readonly Dictionary<string, string> _predefinedTypeAnnotation = new Dictionary<string, string>()
        {
            {"refAllele", "string"},
            {"altAllele", "string"},
            {"start", "number"},
            {"end", "number"}
        };
        internal readonly List<string> JsonKeys = new List<string> {"refAllele", "altAllele"};
        internal readonly List<string> IntervalJsonKeys = new List<string> {"start", "end"};

        public SaJsonSchema JsonSchema;
        public SaJsonSchema IntervalJsonSchema;


        internal CustomAnnotationsParser(StreamReader streamReader, IDictionary<string, IChromosome> refChromDict)
        {
            _reader = streamReader;
            _refChromDict = refChromDict;
            _intervals = new List<CustomInterval>();
        }

        public static CustomAnnotationsParser Create(StreamReader streamReader, IDictionary<string, IChromosome> refChromDict)
        {
            var parser = new CustomAnnotationsParser(streamReader, refChromDict);

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
            ParseTags();
            ParseCategories();
            ParseDescriptions();
            ParseTypes();
        }

        internal void ParseTitle()
        {
            var line = ReadlineAndCheckPrefix("#title");

            var splits = line.OptimizedSplit('=');
            var value = splits[1].Trim();

            if (CheckJsonTagConflict(value))
                throw new UserErrorException($"{value} is a reserved supplementary annotation tag in Nirvana. Please use a different value.");
            JsonTag = value;
        }

        internal void ParseGenomeAssembly()
        {
            var line = ReadlineAndCheckPrefix("#assembly");
            var splits = line.OptimizedSplit('=');

            Assembly = GenomeAssemblyHelper.Convert(splits[1].Trim());
            if (!_allowedGenomeAssemblies.Contains(Assembly))
                throw new UserErrorException("Only GRCh37 and GRCh38 are accepted.");        
        }

        private void ParseTags()
        {
            var line = ReadlineAndCheckPrefix("#CHROM");

            _tags = line.OptimizedSplit('\t');

            CheckAltAndEndColumns();

            for (int i = _numRequiredColumns; i < _tags.Length; i++)
            {
                JsonKeys.Add(_tags[i]);
                IntervalJsonKeys.Add(_tags[i]);
            }
        }

        private void CheckAltAndEndColumns()
        {
            _numRequiredColumns = 4;
            if (_tags[3] == "ALT")
            {
                _altColumnIndex = 3;

                if (_tags[4] == "END")
                {
                    _endColumnIndex = 4;
                    _numRequiredColumns = 5;
                }
            }
            else
            {
                _endColumnIndex = 3;
            }

            _numAnnotationColumns = _tags.Length - _numRequiredColumns;
        }

        private void ParseCategories()
        {
            var line = ReadlineAndCheckPrefix("#categories");
            var splits = line.OptimizedSplit('\t');
            if (splits.Length != _tags.Length) throw new UserErrorException("#categories row must have the same number of columns as the #CHROM row");

            Categories = new CustomAnnotationCategories[_numAnnotationColumns];
            for (int i = 0; i < _numAnnotationColumns; i++)
            {
                switch (splits[i + _numRequiredColumns])
                {
                    case "AlleleCount":
                        Categories[i] = CustomAnnotationCategories.AlleleCount;
                        break;
                    case "AlleleNumber":
                        Categories[i] = CustomAnnotationCategories.AlleleNumber;
                        break;
                    case "AlleleFrequency":
                        Categories[i] = CustomAnnotationCategories.AlleleFrequency;
                        break;
                    case "Prediction":
                        Categories[i] = CustomAnnotationCategories.Prediction;
                        break;
                    default:
                        Categories[i] = CustomAnnotationCategories.Unknown;
                        break;
                }
            }
        }

        private void ParseDescriptions()
        {
            var line = ReadlineAndCheckPrefix("#descriptions");
            var splits = line.OptimizedSplit('\t');
            if (splits.Length != _tags.Length) throw new UserErrorException("#descriptions row must have the same number of columns as the #CHROM row");

            Descriptions = new string[_numAnnotationColumns];
            for (int i = 0; i < _numAnnotationColumns; i++)
            {
                if (splits[i + _numRequiredColumns] == ".") Descriptions[i] = null;
                else Descriptions[i] = splits[i + _numRequiredColumns];
            }
        }

        private void ParseTypes()
        {
            var line = ReadlineAndCheckPrefix("#type");
            var splits = line.OptimizedSplit('\t');
            if (splits.Length != _tags.Length) throw new UserErrorException("#types row must have the same number of columns as the #CHROM row");

            Types = new CustomAnnotationType[_numAnnotationColumns];
            for (int i = 0; i < _numAnnotationColumns; i++)
            {
                switch (splits[i + _numRequiredColumns])
                {
                    case "bool":
                        Types[i] = CustomAnnotationType.Bool;
                        break;
                    case "string":
                        Types[i] = CustomAnnotationType.String;
                        break;
                    case "number":
                        Types[i] = CustomAnnotationType.Number;
                        break;
                    default:
                        throw new UserErrorException("Invalid value for type column. Valid values are bool, string, number.");
                }
            }
        }

        public IEnumerable<CustomItem> GetItems()
        {
            using (_reader)
            {       
                string line;
                while ((line = _reader.ReadLine()) != null)
                {
                    if (line.IsWhiteSpace()) continue;

                    var item = ExtractItems(line);
                    if (item == null) continue;
                    yield return item;
                }
            }
        }

        private void InitiateSchema()
        {
            if (_altColumnIndex != -1) JsonSchema = SaJsonSchema.Create(new StringBuilder(), JsonTag, DataType, JsonKeys);
            if (_endColumnIndex != -1) IntervalJsonSchema = SaJsonSchema.Create(new StringBuilder(), JsonTag, DataType, IntervalJsonKeys);
        }

        private void AddPredefinedTypeAnnotation()
        {
            foreach (var (jsonKey, valueType) in _predefinedTypeAnnotation)
            {
                JsonSchema?.AddAnnotation(jsonKey, new SaJsonKeyAnnotation {Type=valueType});
                IntervalJsonSchema?.AddAnnotation(jsonKey, new SaJsonKeyAnnotation { Type = valueType });
            }
        }

        private void AddHeaderAnnotation()
        {
            for (int i = 0;  i < _numAnnotationColumns; i++)
            {
                var annotation = new SaJsonKeyAnnotation

                {
                    Type = Types[i].ToJsonTypeString(),
                    Category = Categories[i] == CustomAnnotationCategories.Unknown ? null : Categories[i].ToString(),
                    Description = Descriptions[i]
                };
    

                JsonSchema?.AddAnnotation(_tags[i + _numRequiredColumns], annotation);
                IntervalJsonSchema?.AddAnnotation(_tags[i + _numRequiredColumns], annotation);
            }
        }

        private CustomItem ExtractItems(string line)
        {
            var splits = line.OptimizedSplit('\t');
            if (splits.Length != _tags.Length)
                throw new UserErrorException($"Column number mismatch!! Header has {_tags.Length} columns but line contains {splits.Length}");

            string chromosome = splits[0];
            if (!_refChromDict.ContainsKey(chromosome)) return null;

            var chrom = _refChromDict[chromosome];
            int position = int.Parse(splits[1]);
            string refAllele = splits[2];

            var values = new string[_numAnnotationColumns];

            for (var i = 0; i < _numAnnotationColumns; i++)
            {
                values[i] = splits[i + _numRequiredColumns];      
            }


            List<string> itemValues;
            if (_endColumnIndex != -1 && splits[_endColumnIndex] != ".")
            {
                itemValues = new List<string> { splits[1], splits[_endColumnIndex] };
                itemValues.AddRange(values);
                _intervals.Add(new CustomInterval(chrom, itemValues, IntervalJsonSchema));
                return null;
            }

            string altAllele = splits[_altColumnIndex];
            itemValues = new List<string> {refAllele, altAllele};
            itemValues.AddRange(values);
            return new CustomItem(chrom, position, itemValues, JsonSchema);
        }

        public List<CustomInterval> GetCustomIntervals() => _intervals.Count > 0 ? _intervals : null;

        internal string ReadlineAndCheckPrefix(string prefix)
        {
            var line = _reader.ReadLine();
            if (line != null && !line.StartsWith(prefix))
                throw new UserErrorException($"TSV file is required to have {prefix}");

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

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}