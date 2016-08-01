using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Illumina.DataDumperImport.DataStructures
{
    public sealed class ObjectValue : AbstractData, IEnumerable<AbstractData>
    {
        private readonly List<AbstractData> _keyValuePairs = new List<AbstractData>();

        #region IEnumerable<T> pseudo-implementation

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<AbstractData> GetEnumerator()
        {
            return ((IEnumerable<AbstractData>)_keyValuePairs).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        /// adds an abstract key/value pair to the current object
        /// </summary>
        public void Add(AbstractData keyValue)
        {
            _keyValuePairs.Add(keyValue);
        }

        /// <summary>
        /// dumps the data contained within this value
        /// </summary>
        internal override string DumpData(int indentLen, bool isLastChild)
        {
            var sb = new StringBuilder();

            int lastIndex = _keyValuePairs.Count - 1;
            for (int valueIndex = 0; valueIndex < _keyValuePairs.Count; valueIndex++)
            {
                sb.AppendLine(_keyValuePairs[valueIndex].DumpData(indentLen + 3, valueIndex == lastIndex));
            }

            return sb.ToString();
        }

        /// <summary>
        /// returns the string if it exists, null otherwise
        /// </summary>
        internal override string Search(string[] searchKeys, int currentKeyIndex)
        {
            // Console.WriteLine("ObjectValue: desired key: {0}, index: {1}", searchKeys[currentKeyIndex], currentKeyIndex);

            string nextKey = searchKeys[currentKeyIndex];
            AbstractData nextKeyValue = _keyValuePairs.FirstOrDefault(currentPair => currentPair.Key == nextKey);

            return nextKeyValue?.Search(searchKeys, currentKeyIndex);
        }

        /// <summary>
        /// returns the object value if it exists, null otherwise
        /// </summary>
        internal override ObjectValue SearchObjectValue(string[] searchKeys, int currentKeyIndex)
        {
            // Console.WriteLine("ObjectValue: desired key: {0}, index: {1}", searchKeys[currentKeyIndex], currentKeyIndex);

            bool isLastKey = currentKeyIndex == searchKeys.Length - 1;
            string lastKey = searchKeys[searchKeys.Length - 1];

            string nextKey = searchKeys[currentKeyIndex];
            AbstractData nextKeyValue = _keyValuePairs.FirstOrDefault(currentPair => currentPair.Key == nextKey);

            if (isLastKey)
            {
                // Console.WriteLine("Last key!");
                if ((nextKeyValue == null) || (nextKeyValue.Key != lastKey) || !(nextKeyValue is ObjectKeyValue)) return null;
                return ((ObjectKeyValue)nextKeyValue).Value;
            }

            return nextKeyValue?.SearchObjectValue(searchKeys, currentKeyIndex);
        }

        /// <summary>
        /// returns the object value array if it exists, null otherwise
        /// </summary>
        internal override List<AbstractData> SearchObjectValues(string[] searchKeys, int currentKeyIndex)
        {
            // Console.WriteLine("ObjectValue: desired key: {0}, index: {1}", searchKeys[currentKeyIndex], currentKeyIndex);

            string nextKey = searchKeys[currentKeyIndex];
            AbstractData nextKeyValue = _keyValuePairs.FirstOrDefault(currentPair => currentPair.Key == nextKey);

            return nextKeyValue?.SearchObjectValues(searchKeys, currentKeyIndex);
        }
    }

    public sealed class ReferenceStringValue : AbstractData
    {
        public string Value;

        /// <summary>
        /// adds an abstract key/value pair to the current object
        /// </summary>
        public void Add(AbstractData keyValue)
        {
            var stringValue = keyValue as StringKeyValue;

            if (stringValue == null)
            {
                throw new ApplicationException(
                    $"Expected a StringKeyValue when setting the ReferenceStringValue, but found: {keyValue.GetType()}");
            }

            Value = stringValue.Key;
        }

        /// <summary>
        /// dumps the data contained within this value
        /// </summary>
        internal override string DumpData(int indentLen, bool isLastChild)
        {
            var indent = new string(' ', indentLen);
            return $"{indent}{Value}{(isLastChild ? "" : ",")}";
        }

        /// <summary>
        /// returns the string if it exists, null otherwise
        /// </summary>
        internal override string Search(string[] key, int keyIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// returns the object value if it exists, null otherwise
        /// </summary>
        internal override ObjectValue SearchObjectValue(string[] searchKeys, int currentKeyIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// returns the object value array if it exists, null otherwise
        /// </summary>
        internal override List<AbstractData> SearchObjectValues(string[] searchKeys, int currentKeyIndex)
        {
            throw new NotImplementedException();
        }
    }
}
