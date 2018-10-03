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

        private const int NumRequiredColumns = 5;
        private readonly List<CustomInterval> _intervals;

        public CustomAnnotationsParser(StreamReader streamReader, IDictionary<string, IChromosome> refChromDict)
        {
            _reader = streamReader;
            _refChromDict = refChromDict;

            _intervals = new List<CustomInterval>();

            var firstLine = _reader.ReadLine();
            if (!firstLine.StartsWith("#title"))
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
            var position = int.Parse(splits[1]);//we have to get it from RSPOS in info
            var refAllele = splits[2];
            var altAllele = splits[3];
            
            var fieldValues = new Dictionary<string, string>();
            for (var i = NumRequiredColumns; i < splits.Length; i++)
            {
                var value = splits[i];
                if (string.IsNullOrEmpty(value) || value ==".") continue;
                fieldValues[_tags[i]] = splits[i];
            }

            //for small variants, the end column should always be "."
            if (splits[4] != ".")
            {
                int end = int.Parse(splits[4]);
                _intervals.Add(new CustomInterval(chrom, position, end, fieldValues));
                return null;
            }

            if (fieldValues.Count == 0) return null;
            return new CustomItem(chrom, position, refAllele, altAllele, fieldValues);
        }

        public List<CustomInterval> GetCustomIntervals()
        {
            return _intervals.Count > 0 ? _intervals : null;
        }

        internal void ParseHeaderLine(string line)
        {
            if (line.StartsWith("#CHROM"))
            {
                _tags = line.OptimizedSplit('\t');
            }
            //todo: parse categories and description for each field. Needed for Json Schema.
            //if (line.StartsWith("#categories")) GetFieldCategories(line);
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