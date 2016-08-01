using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures
{
    public class SupplementaryInterval : AnnotationInterval, IComparable<SupplementaryInterval>, ISupplementaryInterval
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

        // constructor
        public SupplementaryInterval(int start, int end, string refName, string altAllele, VariantType variantType, string source,
            Dictionary<string, int> intValues = null, Dictionary<string, double> doubleValues = null,
            Dictionary<string, double> freqValues = null, Dictionary<string, string> stringValues = null, List<string> boolValues = null, Dictionary<string, IEnumerable<string>> stringLists = null)
        {
            Start = start;
            End = end;
            //ReferenceName = refName;
            AlternateAllele = altAllele;
            VariantType = variantType;
            Source = source;

            _intValues = intValues ?? new Dictionary<string, int>();

            _boolList = boolValues ?? new List<string>();
            _doubleValues = doubleValues ?? new Dictionary<string, double>();
            _freqValues = freqValues ?? new Dictionary<string, double>();
            _stringValues = stringValues ?? new Dictionary<string, string>();
            _stringLists = stringLists ?? new Dictionary<string, IEnumerable<string>>();

            var chromosomeRenamer = AnnotationLoader.Instance.ChromosomeRenamer;
	        ReferenceName = chromosomeRenamer.GetEnsemblReferenceName(refName);
        }

        #region compareFunctions

        public int CompareTo(SupplementaryInterval other)
        {
            return ReferenceName != other.ReferenceName
                ? string.Compare(ReferenceName, other.ReferenceName, StringComparison.Ordinal)
                : Start.CompareTo(other.Start);
        }

        public override bool Equals(object other)
        {
            var otherItem = other as SupplementaryInterval;
            if (otherItem == null) return false;

            return ReferenceName.Equals(otherItem.ReferenceName)
                   && Start.Equals(otherItem.Start)
                   && End.Equals(otherItem.End)
                   && VariantType.Equals(otherItem.VariantType)
                   && Source.Equals(otherItem.Source);
        }

        public override int GetHashCode()
        {
            var hashCode = Start.GetHashCode() ^ ReferenceName.GetHashCode();
            hashCode = (hashCode * 397) ^ End.GetHashCode();
            hashCode = (hashCode * 397) ^ VariantType.GetHashCode();
            hashCode = (hashCode * 397) ^ Source.GetHashCode();
            hashCode = (hashCode * 397) ^ AlternateAllele.GetHashCode();

            return hashCode;
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

        public static SupplementaryInterval Read(ExtendedBinaryReader reader)
        {
			var referenceName = reader.ReadAsciiString();
			var start = reader.ReadInt();
            var end = reader.ReadInt();
            var alternateAllele = reader.ReadAsciiString();
            var sequenceAlteration = (VariantType)reader.ReadByte();
            var source = reader.ReadAsciiString();

            // strings
            var count = reader.ReadInt();
            var stringValues = new Dictionary<string, string>(count);

            for (int i = 0; i < count; i++)
            {
                var key = reader.ReadAsciiString();
                var value = reader.ReadAsciiString();
                stringValues[key] = value;
            }

            // integers
            count = reader.ReadInt();
            var intValues = new Dictionary<string, int>(count);

            for (int i = 0; i < count; i++)
            {
                var key = reader.ReadAsciiString();
                var value = reader.ReadInt();
                intValues[key] = value;
            }

            // booleans
            count = reader.ReadInt();
            var boolList = new List<string>(count);

            for (int i = 0; i < count; i++) boolList.Add(reader.ReadAsciiString());

            // doubles
            count = reader.ReadInt();
            var doubleValues = new Dictionary<string, double>(count);

            for (int i = 0; i < count; i++)
            {
                var key = reader.ReadAsciiString();
                var value = reader.ReadDouble();
                doubleValues[key] = value;
            }

            // frequencies
            count = reader.ReadInt();
            var freqValues = new Dictionary<string, double>(count);

            for (int i = 0; i < count; i++)
            {
                var key = reader.ReadAsciiString();
                var value = reader.ReadDouble();
                freqValues[key] = value;
            }
            //stringLists
            count = reader.ReadInt();
            var stringLists = new Dictionary<string, IEnumerable<string>>(count);
            for (int i = 0; i < count; i++)
            {
                var key = reader.ReadAsciiString();
                int valueCount = reader.ReadInt();
                var values = ReadStringLists(reader, valueCount);
                stringLists[key] = values;
            }

            return new SupplementaryInterval(start, end, referenceName, alternateAllele, sequenceAlteration, source, intValues, doubleValues, freqValues, stringValues, boolList, stringLists);
        }

        private static List<string> ReadStringLists(ExtendedBinaryReader reader, int count)
        {
            var values = new List<string>();
            for (int i = 0; i < count; i++)
                values.Add(reader.ReadAsciiString());
            return values;
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteAsciiString(ReferenceName);//reference name
            writer.WriteInt(Start); // start
            writer.WriteInt(End); // end
            writer.WriteAsciiString(AlternateAllele);// alternate allele
            writer.WriteByte((byte)VariantType); // variant type
            writer.WriteAsciiString(Source); // source

            // write the dictionaries

            writer.WriteInt(_stringValues.Count);// size of string values dict
            foreach (var keyValue in _stringValues)
            {
                writer.WriteAsciiString(keyValue.Key);
                writer.WriteAsciiString(keyValue.Value);
            }

            writer.WriteInt(_intValues.Count);// size of int values dict
            foreach (var keyValue in _intValues)
            {
                writer.WriteAsciiString(keyValue.Key);
                writer.WriteInt(keyValue.Value);
            }

            writer.WriteInt(_boolList.Count);// size of bool values list
            foreach (var value in _boolList)
                writer.WriteAsciiString(value);

            writer.WriteInt(_doubleValues.Count);// size of double values dict
            foreach (var keyValue in _doubleValues)
            {
                writer.WriteAsciiString(keyValue.Key);
                writer.WriteDouble(keyValue.Value);
            }

            writer.WriteInt(_freqValues.Count);// size of frequency values dict
            foreach (var keyValue in _freqValues)
            {
                writer.WriteAsciiString(keyValue.Key);
                writer.WriteDouble(keyValue.Value);
            }

            writer.WriteInt(_stringLists.Count);// size of stringLists values dict
            foreach (var keyValue in _stringLists)
            {
                writer.WriteAsciiString(keyValue.Key);
                writer.WriteInt(keyValue.Value.Count());
                foreach (var value in keyValue.Value)
                {
                    writer.WriteAsciiString(value);
                }
            }

        }

        public string GetJsonContent()
        {
            var sb = new StringBuilder();
            var jsonObject = new JsonObject(sb);

            // data section

            jsonObject.AddStringValue("chromosome", ReferenceName);
            jsonObject.AddIntValue("begin", Start);
            jsonObject.AddIntValue("end", End);
            jsonObject.AddStringValue("source", Source);
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
