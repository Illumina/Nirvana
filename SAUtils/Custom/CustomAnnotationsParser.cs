using System;
using System.Collections.Generic;
using System.IO;
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
        public string JsonTag { get; }
        private string[] _tags;
        private CustomAnnotationCategories[] _categories;
        private string[] _descriptions;
        private CustomAnnotationTypes[] _types;

        private const int NumRequiredColumns = 5;
        private readonly List<CustomInterval> _intervals;

        public CustomAnnotationsParser(StreamReader streamReader, IDictionary<string, IChromosome> refChromDict)
        {
            _reader = streamReader;
            _refChromDict = refChromDict;

            _intervals = new List<CustomInterval>();

            var firstLine = _reader.ReadLine();
            if (firstLine != null && !firstLine.StartsWith("#title"))
                throw new UserErrorException("TSV file is required to begin with #title");

            var splits = firstLine.OptimizedSplit('=');
            var value = splits[1].Trim();
            if (CheckJsonTagConflict(value))
                throw new UserErrorException($"{value} is a reserved supplementary annotation tag in Nirvana. Please use a different value.");
            JsonTag = value;
        }

        public IEnumerable<CustomItem> GetItems()
        {
            using (_reader)
            {
                string line;
                while ((line = _reader.ReadLine()) != null)
                {
                    if (line.IsWhiteSpace()) continue;
                    if (line.OptimizedStartsWith('#'))
                    {
                        ParseHeaderLine(line);
                        continue;
                    }
                    // check if tags, categories and description has same number of items
                    if(_tags.Length != _categories.Length || _categories.Length != _descriptions.Length)
                        throw new UserErrorException($"Input file has inconsistent number of tags({_tags.Length}), categories({_categories.Length}) and description({_descriptions.Length}).");

                    var item = ExtractItems(line);
                    if (item == null) continue;
                    yield return item;
                }
            }
        }

        
        internal CustomItem ExtractItems(string line)
        {
            var splits = line.OptimizedSplit('\t');
            if (splits.Length != _tags.Length)
                throw new UserErrorException($"Column number mismatch!! Header has {_tags.Length} columns but line contains {splits.Length}");

            var chromosome = splits[0];
            if (!_refChromDict.ContainsKey(chromosome)) return null;

            var chrom = _refChromDict[chromosome];
            var position = int.Parse(splits[1]);
            var refAllele = splits[2];
            var altAllele = splits[3];
            
            var stringValues = new Dictionary<string, string>();
            var boolValues   = new Dictionary<string, bool>();
            var numValues    = new Dictionary<string, double>();

            for (var i = NumRequiredColumns; i < splits.Length; i++)
            {
                var value = splits[i];
                if (string.IsNullOrEmpty(value) || value ==".") continue;
                switch (_types[i])
                {
                    case CustomAnnotationTypes.String:
                        stringValues[_tags[i]] = splits[i];
                        break;
                    case CustomAnnotationTypes.Bool:
                        boolValues[_tags[i]] = splits[i]=="true";
                        break;
                    case CustomAnnotationTypes.Number:
                        numValues[_tags[i]] = double.Parse(splits[i]);
                        break;
                }
                
            }

            //for small variants, the end column should always be "."
            if (splits[4] != ".")
            {
                int end = int.Parse(splits[4]);
                _intervals.Add(new CustomInterval(chrom, position, end, stringValues, boolValues, numValues));
                return null;
            }

            if (stringValues.Count == 0 && boolValues.Count==0 && numValues.Count==0) return null;
            return new CustomItem(chrom, position, refAllele, altAllele, stringValues,boolValues, numValues);
        }

        public List<CustomInterval> GetCustomIntervals()
        {
            return _intervals.Count > 0 ? _intervals : null;
        }

        internal void ParseHeaderLine(string line)
        {
            var splits = line.OptimizedSplit('\t');
            switch (splits[0])
            {
                case "#CHROM":
                    _tags = splits;
                    break;
                case "#type":
                    ParseTypes(splits);
                    break;
                case "#categories":
                    ParseCategories(splits);
                    break;
                case "#descriptions":
                    ParseDescriptions(splits);
                    break;
            }
            
        }

        private void ParseTypes(string[] splits)
        {
            _types = new CustomAnnotationTypes[splits.Length];
            for (int i = NumRequiredColumns; i < splits.Length; i++)
            {
                switch (splits[i])
                {
                    case "bool":
                        _types[i] = CustomAnnotationTypes.Bool;
                        break;
                    case "string":
                        _types[i] = CustomAnnotationTypes.String;
                        break;
                    case "number":
                        _types[i] = CustomAnnotationTypes.Number;
                        break;
                    default:
                        throw new UserErrorException("Invalid value for type column. Valid values are bool, string, number.");
                }
            }
        }

        private void ParseDescriptions(string[] splits)
        {
            _descriptions = new string[splits.Length];
            for (int i = NumRequiredColumns; i < splits.Length; i++)
            {
                if (splits[i] == ".") _descriptions[i] = null;
                else _descriptions[i] = splits[i];
            }
        }

        private void ParseCategories(string[] splits)
        {
            _categories = new CustomAnnotationCategories[splits.Length];
            for(int i = NumRequiredColumns; i < splits.Length; i++)
            {
                switch (splits[i])
                {
                    case "AlleleCount":
                        _categories[i] = CustomAnnotationCategories.AlleleCount;
                        break;
                    case "AlleleNumber":
                        _categories[i] = CustomAnnotationCategories.AlleleNumber;
                        break;
                    case "AlleleFrequency":
                        _categories[i] = CustomAnnotationCategories.AlleleFrequency;
                        break;
                    case "Prediction":
                        _categories[i] = CustomAnnotationCategories.Prediction;
                        break;
                    default:
                        _categories[i] = CustomAnnotationCategories.Unknown;
                        break;
                }

            }
            
        }

        private bool CheckJsonTagConflict(string value)
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