using System.Collections.Generic;
using System.Linq;
using System.Text;
using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.IO
{
    public sealed class JsonObject
    {
        private readonly StringBuilder _sb;
        private bool _needsComma;
        private int _nestedLevel;

        public const char Comma = ',';
        private const char DoubleQuote = '\"';
        public const char OpenBracket = '[';
        public const char CloseBracket = ']';
        public const char OpenBrace = '{';
        public const char CloseBrace = '}';
        private const string ColonString = "\":";

        public JsonObject(StringBuilder sb) => _sb = sb;

        private void AddKey(string description)
        {
            _sb.Append(DoubleQuote);
            _sb.Append(description);
            _sb.Append(ColonString);
        }

        public void StartObjectWithKey(string objectKey)
        {
            if (_needsComma) _sb.Append(Comma);

            _sb.Append(DoubleQuote);
            _sb.Append(objectKey);
            _sb.Append(ColonString);
            _sb.Append(OpenBrace);

            _needsComma = false;
            _nestedLevel++;
        }

        public bool AddBoolValue(string description, bool b, bool outputFalse = false)
        {
            // we do not want to print out false flags by default.
            if (!b && !outputFalse) return false;

            if (_needsComma) _sb.Append(Comma);
            AddKey(description);

            _sb.Append(b ? "true" : "false");
            _needsComma = true;

            return true;
        }

        public bool AddIntValue(string description, int? i)
        {
            if (i == null) return false;

            if (_needsComma) _sb.Append(Comma);
            AddKey(description);

            _sb.Append(i);
            _needsComma = true;

            return true;
        }

        public void AddIntValues(string description, int[] values)
        {
            if (values == null || values.Length == 0) return;

            // removing '.'s from the list of values
            var valueList = values.Select(value => value.ToString()).ToList();

            AddStringValues(description, valueList, false);
            _needsComma = true;
        }

        public bool AddDoubleValue(string description, double? d, string format = "0.####")
        {
            if (d == null) return false;

            if (_needsComma) _sb.Append(Comma);
            AddKey(description);
            _sb.Append(d.Value.ToString(format));
            _needsComma = true;

            return true;
        }

        public void AddDoubleValues(string description, double[] values, string format = "0.####")
        {
            if (values == null || values.Length == 0) return;

            var valueList = values.Select(value => value.ToString(format)).ToList();

            AddStringValues(description, valueList, false);
            _needsComma = true;
        }

        public bool AddStringValue(string description, string s, bool useQuote = true)
        {
            if (string.IsNullOrEmpty(s) || s == ".") return false;

            if (_needsComma) _sb.Append(Comma);
            AddKey(description);

            if (useQuote) _sb.Append(DoubleQuote);
            _sb.Append(s);
            if (useQuote) _sb.Append(DoubleQuote);
            _needsComma = true;

            return true;
        }

        public bool AddStringValues(string description, IEnumerable<string> values, bool useQuote = true)
        {
            if (values == null) return false;

            var validEntries = new List<string>();
            foreach (string value in values) if (value != ".") validEntries.Add(value);

            if (validEntries.Count == 0) return false;

            if (_needsComma) _sb.Append(Comma);
            AddKey(description);
            _sb.Append(OpenBracket);

            var needsComma = false;

            foreach (string value in validEntries)
            {
                if (needsComma) _sb.Append(Comma);
                if (useQuote) _sb.Append(DoubleQuote);
                _sb.Append(value);
                if (useQuote) _sb.Append(DoubleQuote);
                needsComma = true;
            }

            _sb.Append(CloseBracket);
            _needsComma = true;

            return true;
        }

        public void AddObjectValue<T>(string description, T value) where T : IJsonSerializer
        {
            if (value == null) return;

            if (_needsComma) _sb.Append(Comma);
            AddKey(description);
            
            value.SerializeJson(_sb);

            _needsComma = true;
        }

        public bool AddObjectValues<T>(string description, IEnumerable<T> values) where T : IJsonSerializer
        {
            if (values == null) return false;

            if (_needsComma) _sb.Append(Comma);
            AddKey(description);
            _sb.Append(OpenBracket);

            var needsComma = false;

            foreach (var value in values)
            {
                // comma handling
                if (needsComma) _sb.Append(Comma);
                else needsComma = true;
                value.SerializeJson(_sb);
            }
            
            _sb.Append(CloseBracket);
            _needsComma = true;

            return true;
        }

        public void StartObject()
        {
            _sb.Append(OpenBrace);
            _needsComma = false;
            _nestedLevel++;
        }

        public void EndObject()
        {
            _sb.Append(CloseBrace);
            _needsComma = true;
            _nestedLevel--;
        }

        public void EndAllObjects()
        {
            _sb.Append(CloseBrace, _nestedLevel);
        }
    }
}