using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VariantAnnotation.DataStructures.Intervals;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
    public sealed class SupplementaryInterval : AnnotationInterval, IEquatable<SupplementaryInterval>, IComparable<SupplementaryInterval>, ISupplementaryInterval
    {
        #region members

        int ISupplementaryInterval.Start => Start;
        int ISupplementaryInterval.End => End;
        public string ReferenceName { get; }
        public string AlternateAllele { get; }
        public VariantType VariantType { get; }
        public string Source { get; }
        private readonly Dictionary<string, string> _stringValues;
        public IReadOnlyDictionary<string, string> StringValues => _stringValues;
        private readonly Dictionary<string, int> _intValues;
        public IReadOnlyDictionary<string, int> IntValues => _intValues;
        private readonly List<string> _boolList;
        public IEnumerable<string> BoolValues => _boolList;
        private readonly Dictionary<string, double> _doubleValues;
        public IReadOnlyDictionary<string, double> DoubleValues => _doubleValues;
        private readonly Dictionary<string, double> _freqValues;
        public IReadOnlyDictionary<string, double> PopulationFrequencies => _freqValues;
        private readonly Dictionary<string, IEnumerable<string>> _stringLists;
        public IReadOnlyDictionary<string, IEnumerable<string>> StringLists => _stringLists;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public SupplementaryInterval(int start, int end, string refName, string altAllele, VariantType variantType,
            string source, IChromosomeRenamer renamer, Dictionary<string, int> intValues = null,
            Dictionary<string, double> doubleValues = null, Dictionary<string, double> freqValues = null,
            Dictionary<string, string> stringValues = null, List<string> boolValues = null,
            Dictionary<string, IEnumerable<string>> stringLists = null) : base(start, end)
        {
            Start           = start;
            End             = end;
            AlternateAllele = altAllele;
            VariantType     = variantType;
            Source          = source;

            _intValues = intValues ?? new Dictionary<string, int>();

            _boolList     = boolValues ?? new List<string>();
            _doubleValues = doubleValues ?? new Dictionary<string, double>();
            _freqValues   = freqValues ?? new Dictionary<string, double>();
            _stringValues = stringValues ?? new Dictionary<string, string>();
            _stringLists  = stringLists ?? new Dictionary<string, IEnumerable<string>>();

            ReferenceName = renamer.GetEnsemblReferenceName(refName);
        }

        #region IComparable/IEquatable methods

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return ReferenceName.GetHashCode() ^ Start.GetHashCode() ^ End.GetHashCode() ^ VariantType.GetHashCode() ^
                   Source.GetHashCode() ^ AlternateAllele.GetHashCode();
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }

        public int CompareTo(SupplementaryInterval other)
        {
            return ReferenceName != other.ReferenceName
                ? string.Compare(ReferenceName, other.ReferenceName, StringComparison.Ordinal)
                : Start.CompareTo(other.Start);
        }

        public bool Equals(SupplementaryInterval value)
        {
            if (this == null) throw new NullReferenceException();
            if (value == null) return false;
            if (this == value) return true;

            return Start == value.Start && End == value.End && ReferenceName.Equals(value.ReferenceName) &&
                   VariantType == value.VariantType && Source.Equals(value.Source) &&
                   AlternateAllele.Equals(value.AlternateAllele);
        }

        #endregion

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
            var sb = new StringBuilder();
            var jsonObject = new JsonObject(sb);

            // data section
            jsonObject.AddStringValue("chromosome", ReferenceName);
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
                jsonObject.AddBoolValue(kvp, true, true, "true");
            }
            foreach (var kvp in StringLists)
            {
                jsonObject.AddStringValues(kvp.Key, kvp.Value);
            }


            return sb.ToString();
        }
    }
}
