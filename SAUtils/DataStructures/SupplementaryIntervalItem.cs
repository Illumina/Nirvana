using System.Collections.Generic;
using System.Linq;
using CommonUtilities;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class SupplementaryIntervalItem : IChromosomeInterval
    {
        public int Start { get; }
        public int End { get; }
        public IChromosome Chromosome { get; }
        private string AlternateAllele { get; }
        public VariantType VariantType { get; }
        public string Source { get; }
        private readonly Dictionary<string, string> _stringValues;
        public IReadOnlyDictionary<string, string> StringValues => _stringValues;
        private readonly Dictionary<string, int> _intValues;
        public IReadOnlyDictionary<string, int> IntValues => _intValues;
        private readonly List<string> _boolList;
        private IEnumerable<string> BoolValues => _boolList;
        private readonly Dictionary<string, double> _doubleValues;
        private IReadOnlyDictionary<string, double> DoubleValues => _doubleValues;
        private readonly Dictionary<string, double> _freqValues;
        public IReadOnlyDictionary<string, double> PopulationFrequencies => _freqValues;
        private readonly Dictionary<string, IEnumerable<string>> _stringLists;
        private IReadOnlyDictionary<string, IEnumerable<string>> StringLists => _stringLists;


        public SupplementaryIntervalItem(IChromosome chromsome, int start, int end,  string altAllele, VariantType variantType,
            string source,  Dictionary<string, int> intValues = null,
            Dictionary<string, double> doubleValues = null, Dictionary<string, double> freqValues = null,
            Dictionary<string, string> stringValues = null, List<string> boolValues = null,
            Dictionary<string, IEnumerable<string>> stringLists = null) 
        {
            Start = start;
            End = end;
            AlternateAllele = altAllele;
            VariantType = variantType;
            Source = source;

            _intValues = intValues ?? new Dictionary<string, int>();

            _boolList = boolValues ?? new List<string>();
            _doubleValues = doubleValues ?? new Dictionary<string, double>();
            _freqValues = freqValues ?? new Dictionary<string, double>();
            _stringValues = stringValues ?? new Dictionary<string, string>();
            _stringLists = stringLists ?? new Dictionary<string, IEnumerable<string>>();

            Chromosome = chromsome;
        }

        public void AddStringList(string key, IEnumerable<string> values)
        {
            var vals = values.ToList();
            if (vals.Any()) _stringLists[key] = vals;
        }

        public void AddStringValue(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                _stringValues[key] = value;
        }

        public void AddIntValue(string key, int value)
        {
            _intValues[key] = value;
        }

        public void AddBoolValue(string value)
        {
            _boolList.Add(value);
        }

        public void AddFrequencyValue(string key, double value)
        {
            _freqValues[key] = value;
        }

        public string GetJsonString()
        {
            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            // data section
            jsonObject.AddStringValue("chromosome", Chromosome.EnsemblName);
            jsonObject.AddIntValue("begin", Start);
            jsonObject.AddIntValue("end", End);
            jsonObject.AddStringValue("variantType", VariantType.ToString());

            foreach (var kvp in PopulationFrequencies)
            {
                if (kvp.Value > 0)
                {
                    jsonObject.AddStringValue(kvp.Key, kvp.Value.ToString("0.#####"), false);
                }
            }

            foreach (var kvp in StringValues)
            {
                jsonObject.AddStringValue(kvp.Key, kvp.Value);
            }

            foreach (var kvp in IntValues)
            {
                jsonObject.AddIntValue(kvp.Key, kvp.Value);
            }

            foreach (var kvp in DoubleValues)
            {
                if (kvp.Value > 0)
                {
                    jsonObject.AddStringValue(kvp.Key, kvp.Value.ToString("0.#####"), false);
                }
            }

            foreach (var kvp in BoolValues)
            {
                jsonObject.AddBoolValue(kvp, true);
            }
            foreach (var kvp in StringLists)
            {
                jsonObject.AddStringValues(kvp.Key, kvp.Value.ToArray());
            }

            return StringBuilderCache.GetStringAndRelease(sb);
        }
    }
}
