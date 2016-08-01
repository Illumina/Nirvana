using System.Collections.Generic;
using System.Linq;
using System.Text;
using VariantAnnotation.DataStructures.JsonAnnotations;

namespace VariantAnnotation.FileHandling.JSON
{
    public class JsonObject
    {
        #region members

        private readonly StringBuilder _sb;
        private bool _needsComma;

        internal const char Comma = ',';
        private const char DoubleQuote = '\"';
        private const char OpenBracket = '[';
        private const char CloseBracket = ']';
        internal const char OpenBrace = '{';
        internal const char CloseBrace = '}';
        private const string ColonString = "\":";

        #endregion

        // constructor
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
        public void AddBoolValue(string description, bool isSet, bool b, string trueString)
        {
            if (!isSet) return;
            if (b == false) return;//we do not want to print out false flags.

            if (_needsComma) _sb.Append(Comma);
            AddKey(description);
            
            _sb.Append(trueString);

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
        /// adds the string KVP to the string builder
        /// </summary>
        public void AddStringValue(string description, string s, bool useQuote = true)
        {
            if (string.IsNullOrEmpty(s) || (s == ".")) return;

			if (_needsComma) _sb.Append(Comma);
            AddKey(description);

            if (useQuote) _sb.Append(DoubleQuote);
            _sb.Append(s);
            if (useQuote) _sb.Append(DoubleQuote);

            _needsComma = true;
        }

        /// <summary>
        /// adds the string KVP to the string builder
        /// </summary>
        public void AddStringValues(string description, string[] values, bool useQuote = true)
        {
            if (values == null) return;
            // removing '.'s from the list of values
            var valueList = values.Where(value => value != ".").ToList();

			AddStringValues(description, valueList, useQuote);
		}

		public void AddStringValues(string description, IEnumerable<string> values, bool useQuote = true)
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

		    for (int i = 1; i < valueList.Count; i++)
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
		public void AddObjectValues(string description, IEnumerable<IJsonSerializer> values)
        {
            if (values == null) return;

            if (_needsComma) _sb.Append(Comma);
            AddKey(description);
            _sb.Append(OpenBracket);

            bool needsComma = false;

            foreach (var value in values)
            {
                // comma handling
                if (needsComma) _sb.Append(Comma);
                else needsComma = true;

                value.SerializeJson(_sb);
            }

            _sb.Append(CloseBracket);
            _needsComma = true;
        }

        /// <summary>
        /// opens a JSON object
        /// </summary>
        public void OpenObject(string description)
        {
            if (_needsComma) _sb.Append(Comma);
            AddKey(description);
            _sb.Append(OpenBrace);
        }

        /// <summary>
        /// closes a JSON object
        /// </summary>
        public void CloseObject()
        {
            _sb.Append(CloseBrace);
        }

        /// <summary>
        /// resets the current JSON object
        /// </summary>
        public void Reset(bool needsComma = false)
        {
            _needsComma = needsComma;
        }
    }
}
