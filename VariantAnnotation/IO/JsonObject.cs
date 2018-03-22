using System;
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

        public const char Comma          = ',';
        private const char DoubleQuote   = '\"';
        private const char OpenBracket   = '[';
        private const char CloseBracket  = ']';
        public const char OpenBrace      = '{';
        public const char CloseBrace     = '}';
        private const string ColonString = "\":";

        public JsonObject(StringBuilder sb) => _sb = sb;

        private void AddKey(string description)
        {
            _sb.Append(DoubleQuote);
            _sb.Append(description);
            _sb.Append(ColonString);
        }

        public void AddBoolValue(string description, bool b, bool outputFalse = false)
        {
            // we do not want to print out false flags by default.
            if (!b && !outputFalse) return;

            if (_needsComma) _sb.Append(Comma);
            AddKey(description);

            _sb.Append(b ? "true" : "false");
            _needsComma = true;
        }

        public void AddIntValue(string description, int? i)
        {
            if (i == null) return;

            if (_needsComma) _sb.Append(Comma);
            AddKey(description);

            _sb.Append(i);
            _needsComma = true;
        }

        public void AddIntValues(string description, int[] values)
        {
            if (values == null || values.Length == 0) return;

            // removing '.'s from the list of values
            var valueList = values.Select(value => value.ToString()).ToList();

            AddStringValues(description, valueList, false);
            _needsComma = true;
        }

        public void AddDoubleValue(string description, double? d, string format = "0.####")
        {
            if (d == null) return;

            if (_needsComma) _sb.Append(Comma);
            AddKey(description);
            _sb.Append(d.Value.ToString(format));
            _needsComma = true;
        }

        public void AddDoubleValues(string description, double[] values, string format = "0.####")
        {
            if (values == null || values.Length == 0) return;

            var valueList = values.Select(value => value.ToString(format)).ToList();

            AddStringValues(description, valueList, false);
            _needsComma = true;
        }

        public void AddStringValue(string description, string s, bool useQuote = true)
        {
            if (string.IsNullOrEmpty(s) || s == ".") return;

            if (_needsComma) _sb.Append(Comma);
            AddKey(description);

            if (useQuote) _sb.Append(DoubleQuote);
            _sb.Append(s);
            if (useQuote) _sb.Append(DoubleQuote);
            _needsComma = true;
        }

        public void AddStringValues(string description, IEnumerable<string> values, bool useQuote = true)
        {
            if (values == null) return;
            var index = 0;

            foreach (var value in values)
            {
                if (value == ".") continue;

                if (index == 0)
                {
                    if (_needsComma) _sb.Append(Comma);
                    AddKey(description);
                    _sb.Append(OpenBracket);

                    if (useQuote) _sb.Append(DoubleQuote);
                    _sb.Append(value);
                    if (useQuote) _sb.Append(DoubleQuote);
                }
                else
                {
                    _sb.Append(Comma);
                    if (useQuote) _sb.Append(DoubleQuote);
                    _sb.Append(value);
                    if (useQuote) _sb.Append(DoubleQuote);
                }
                index++;
            }

            if (index > 0)
                _sb.Append(CloseBracket);
            _needsComma = true;
        }

        public void AddObjectValues<T>(string description, IEnumerable<T> values, bool seperatedByNewLine = false) where T : IJsonSerializer
        {
            if (values == null) return;

            if (_needsComma) _sb.Append(Comma);
            AddKey(description);
            _sb.Append(OpenBracket);

            var needsComma = false;

            foreach (var value in values)
            {
                // comma handling
                if (needsComma) _sb.Append(Comma);
                else needsComma = true;
                if (seperatedByNewLine) _sb.Append("\n");
                value.SerializeJson(_sb);
            }
            if (seperatedByNewLine) _sb.Append("\n");
            _sb.Append(CloseBracket);
            _needsComma = true;
        }

        public void AddGroupedObjectValues<T>(string description, string[] groupDescriptions, params IList<T>[] groups) where T : IJsonSerializer
        {
            if (groupDescriptions == null) return;

            if (groups.Length != groupDescriptions.Length)
                throw new ArgumentException(
                    $"The count ({groupDescriptions.Length}) of descriptions does not match the count ({groups.Length}) of groups");

            if (_needsComma) _sb.Append(Comma);
            AddKey(description);
            _sb.Append(OpenBrace);

            var i = 0;
            Reset();
            foreach (var group in groups)
            {
                i++;
                if (group == null || !group.Any()) continue;
                AddObjectValues(groupDescriptions[i - 1], group);
            }

            _sb.Append(CloseBrace);
            _needsComma = true;
        }

        private void Reset(bool needsComma = false) => _needsComma = needsComma;
    }
}