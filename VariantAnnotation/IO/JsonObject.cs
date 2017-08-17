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

        public JsonObject(StringBuilder sb)
        {
            _sb = sb;
        }

        private void AddKey(string description)
        {
            _sb.Append(DoubleQuote);
            _sb.Append(description);
            _sb.Append(ColonString);
        }

        /// <summary>
        /// adds the boolean KVP to the string builder
        /// </summary>
        public void AddBoolValue(string description, bool b)
        {
            if (b == false) return;//we do not want to print out false flags.

            if (_needsComma) _sb.Append(Comma);
            AddKey(description);

            _sb.Append("true");
            _needsComma = true;
        }

        /// <summary>
        /// adds the string KVP to the string builder
        /// </summary>
        public void AddIntValue(string description, int? i)
        {
            if (i == null) return;

            if (_needsComma) _sb.Append(Comma);
            AddKey(description);

            _sb.Append(i);
            _needsComma = true;
        }

        /// <summary>
        /// Add an integer array kvp to the StringBuilder
        /// </summary>
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

        /// <summary>
        /// adds the string KVP to the string builder
        /// </summary>
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

        /// <summary>
        /// adds the string KVP to the string builder
        /// TODO: Should dot (.) removal actually be part of this method?
        /// </summary>
        public void AddStringValues(string description, string[] values, bool useQuote = true)
        {
            if (values == null) return;

            // removing '.'s from the list of values
            var valueList = values.Where(value => value != ".").ToList();
            AddStringValues(description, valueList, useQuote);
        }

        private void AddStringValues(string description, List<string> valueList, bool useQuote)
        {
            if (valueList.Count == 0) return;

            if (_needsComma) _sb.Append(Comma);
            AddKey(description);
            _sb.Append(OpenBracket);

            if (useQuote) _sb.Append(DoubleQuote);
            _sb.Append(valueList[0]);
            if (useQuote) _sb.Append(DoubleQuote);

            for (var i = 1; i < valueList.Count; i++)
            {
                _sb.Append(Comma);
                if (useQuote) _sb.Append(DoubleQuote);
                _sb.Append(valueList[i]);
                if (useQuote) _sb.Append(DoubleQuote);
            }

            _sb.Append(CloseBracket);
            _needsComma = true;
        }

        /// <summary>
        /// adds the object values to this current JSON object
        /// </summary>
        public void AddObjectValues<T>(string description, IList<T> values,bool seperatedByNewLine = false) where T : IJsonSerializer
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

        /// <summary>
        /// adds the object values to this current JSON object
        /// </summary>
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

        /// <summary>
        /// resets the current JSON object
        /// </summary>
        private void Reset(bool needsComma = false)
        {
            _needsComma = needsComma;
        }
    }
}