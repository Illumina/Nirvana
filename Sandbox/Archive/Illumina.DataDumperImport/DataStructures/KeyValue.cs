using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Illumina.DataDumperImport.DataStructures
{
    public sealed class StringKeyValue : AbstractData
    {
        public readonly string Value;

        // constructor
        public StringKeyValue(string key, string value) : base(key)
        {
            Value = value;
        }

        /// <summary>
        /// dumps the data contained within this key/value pair 
        /// </summary>
        internal override string DumpData(int indentLen, bool isLastChild)
        {
            var indent = new string(' ', indentLen);
            string value = Value == null ? UndefTag : $"'{Value}'";
            return $"{indent}'{Key}' => {value}{(isLastChild ? "" : ",")}";
        }

        /// <summary>
        /// returns the string if it exists, null otherwise
        /// </summary>
        internal override string Search(string[] searchKeys, int currentKeyIndex)
        {
            // check the current key value
            // Console.WriteLine("StringKeyValue: current key: {0}, desired key: {1}, index: {2}", Key, searchKeys[currentKeyIndex], currentKeyIndex);
            bool same = Key == searchKeys[currentKeyIndex];

            // return the appropriate string
            return same ? Value : null;
        }

        /// <summary>
        /// returns the object value if it exists, null otherwise
        /// </summary>
        internal override ObjectValue SearchObjectValue(string[] searchKeys, int currentKeyIndex)
        {
            return null;
        }

        /// <summary>
        /// returns the object value array if it exists, null otherwise
        /// </summary>
        internal override List<AbstractData> SearchObjectValues(string[] searchKeys, int currentKeyIndex)
        {
            return null;
        }
    }

    public sealed class ReferenceKeyValue : AbstractData
    {
        public readonly string Value;

        // constructor
        public ReferenceKeyValue(string key, string value)
            : base(key)
        {
            Value = value;
        }

        /// <summary>
        /// dumps the data contained within this key/value pair 
        /// </summary>
        internal override string DumpData(int indentLen, bool isLastChild)
        {
            var indent = new string(' ', indentLen);
            string value = Value == null ? UndefTag : $"'{Value}'";
            return $"{indent}'{Key}' => {value}{(isLastChild ? "" : ",")}";
        }

        /// <summary>
        /// returns the string if it exists, null otherwise
        /// </summary>
        internal override string Search(string[] searchKeys, int currentKeyIndex)
        {
            // check the current key value
            // Console.WriteLine("StringKeyValue: current key: {0}, desired key: {1}, index: {2}", Key, searchKeys[currentKeyIndex], currentKeyIndex);
            bool same = Key == searchKeys[currentKeyIndex];

            // return the appropriate string
            return same ? Value : null;
        }

        /// <summary>
        /// returns the object value if it exists, null otherwise
        /// </summary>
        internal override ObjectValue SearchObjectValue(string[] searchKeys, int currentKeyIndex)
        {
            return null;
        }

        /// <summary>
        /// returns the object value array if it exists, null otherwise
        /// </summary>
        internal override List<AbstractData> SearchObjectValues(string[] searchKeys, int currentKeyIndex)
        {
            return null;
        }
    }

    public sealed class ObjectKeyValue : AbstractData
    {
        public readonly ObjectValue Value;

        // constructor
        public ObjectKeyValue(string key, ObjectValue value)
            : base(key)
        {
            Value = value;
        }

        /// <summary>
        /// dumps the data contained within this key/value pair 
        /// </summary>
        internal override string DumpData(int indentLen, bool isLastChild)
        {
            var indent = new string(' ', indentLen);
            return string.Format("{0}'{1}' => {{\n{2}{0}}}{3}{4}",
                indent,
                Key,
                Value.DumpData(indentLen, isLastChild),
                Value.DataType == UnknownDataType ? "" : $" [{Value.DataType}]",
                isLastChild ? "" : ",");
        }

        /// <summary>
        /// returns the first child to this object if one is available, null otherwise
        /// </summary>
        public AbstractData GetChild()
        {
            return Value;
        }

        /// <summary>
        /// returns the string if it exists, null otherwise
        /// </summary>
        internal override string Search(string[] searchKeys, int currentKeyIndex)
        {
            // check the current key value
            // Console.WriteLine("ObjectKeyValue: current key: {0}, desired key: {1}, index: {2}", Key, searchKeys[currentKeyIndex], currentKeyIndex);

            // return the appropriate string
            bool same = Key == searchKeys[currentKeyIndex];

            // if (!same) Console.WriteLine("- different: [{0}] - [{1}]", Key, searchKeys[currentKeyIndex]);

            return same ? Value.Search(searchKeys, currentKeyIndex + 1) : null;
        }

        /// <summary>
        /// returns the object value if it exists, null otherwise
        /// </summary>
        internal override ObjectValue SearchObjectValue(string[] searchKeys, int currentKeyIndex)
        {
            return Key == searchKeys[currentKeyIndex] ? Value.SearchObjectValue(searchKeys, currentKeyIndex + 1) : null;
        }

        /// <summary>
        /// returns the object value if it exists, null otherwise
        /// </summary>
        internal override List<AbstractData> SearchObjectValues(string[] searchKeys, int currentKeyIndex)
        {
            return Key == searchKeys[currentKeyIndex] ? Value.SearchObjectValues(searchKeys, currentKeyIndex + 1) : null;
        }
    }

    public sealed class ListObjectKeyValue : AbstractData, IEnumerable<AbstractData>
    {
        public readonly List<AbstractData> Values = new List<AbstractData>();

        // constructor
        public ListObjectKeyValue(string key) : base(key) {}

        #region IEnumerable<T> pseudo-implementation

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<AbstractData> GetEnumerator()
        {
            return ((IEnumerable<AbstractData>)Values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion


        /// <summary>
        /// adds an abstract key/value pair to the current object
        /// (not used)
        /// </summary>
        public void Add(AbstractData keyValue)
        {
            Values.Add(keyValue);
        }

        /// <summary>
        /// dumps the data contained within this key/value pair 
        /// </summary>
        internal override string DumpData(int indentLen, bool isLastChild)
        {
            var indent  = new string(' ', indentLen);
            var indent2 = new string(' ', indentLen + 3);

            var sb = new StringBuilder();
            sb.AppendFormat("{0}'{1}' => [\n", indent, Key);

            int lastIndex = Values.Count - 1;
            for (int valueIndex = 0; valueIndex < Values.Count; valueIndex++)
            {
                var value = Values[valueIndex];

                if (value is ObjectValue)
                {
                    sb.AppendFormat("{0}{{\n{1}{0}}}{2}{3}\n",
                        indent2,
                        value.DumpData(indentLen + 3, valueIndex == lastIndex),
                        value.DataType == UnknownDataType ? "" : $" [{value.DataType}]",
                        valueIndex == lastIndex ? "" : ",");
                }
                else
                {
                    sb.AppendFormat("{0}{1}\n",
                        indent,
                        value.DumpData(indentLen, valueIndex == lastIndex));
                }
            }

            sb.AppendFormat("{0}]{1}", indent, isLastChild ? "" : ",");

            return sb.ToString();
        }

        /// <summary>
        /// returns the string if it exists, null otherwise
        /// </summary>
        internal override string Search(string[] searchKeys, int currentKeyIndex)
        {
            currentKeyIndex++;

            // check the current key value
            // Console.WriteLine("ListObjectKeyValue: key: [{0}], desired key: {1}, index: {2}", Key, searchKeys[currentKeyIndex], currentKeyIndex);

            // convert the key to an integer
            int foundKeyIndex;
            if (!int.TryParse(searchKeys[currentKeyIndex], out foundKeyIndex)) return null;

            // sanity checking
            if ((foundKeyIndex < 0) || (foundKeyIndex >= Values.Count)) return null;

            return Values[foundKeyIndex].Search(searchKeys, currentKeyIndex + 1);
        }

        /// <summary>
        /// returns the object value if it exists, null otherwise
        /// </summary>
        internal override ObjectValue SearchObjectValue(string[] searchKeys, int currentKeyIndex)
        {
            currentKeyIndex++;

            // check the current key value
            // Console.WriteLine("ListObjectKeyValue: key: [{0}], desired key: {1}, index: {2}", Key, searchKeys[currentKeyIndex], currentKeyIndex);

            // convert the key to an integer
            int foundKeyIndex;
            if (!int.TryParse(searchKeys[currentKeyIndex], out foundKeyIndex)) return null;

            // sanity checking
            if ((foundKeyIndex < 0) || (foundKeyIndex >= Values.Count)) return null;

            return Values[foundKeyIndex].SearchObjectValue(searchKeys, currentKeyIndex + 1);
        }

        /// <summary>
        /// returns the object value if it exists, null otherwise
        /// </summary>
        internal override List<AbstractData> SearchObjectValues(string[] searchKeys, int currentKeyIndex)
        {
            bool isLastKey = currentKeyIndex == searchKeys.Length - 1;
            string lastKey = searchKeys[searchKeys.Length - 1];

            if (isLastKey)
            {
                // Console.WriteLine("Last key!");
                return Key != lastKey ? null : Values;
            }

            currentKeyIndex++;

            // check the current key value
            // Console.WriteLine("ListObjectKeyValue: key: [{0}], desired key: {1}, index: {2}", Key, searchKeys[currentKeyIndex], currentKeyIndex);

            // convert the key to an integer
            int foundKeyIndex;
            if (!int.TryParse(searchKeys[currentKeyIndex], out foundKeyIndex)) return null;

            // sanity checking
            if ((foundKeyIndex < 0) || (foundKeyIndex >= Values.Count)) return null;

            return Values[foundKeyIndex].SearchObjectValues(searchKeys, currentKeyIndex + 1);
        }
    }
}
