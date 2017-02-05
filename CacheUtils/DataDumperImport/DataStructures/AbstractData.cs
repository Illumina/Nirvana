using System.Collections.Generic;

namespace CacheUtils.DataDumperImport.DataStructures
{
    public abstract class AbstractData
    {
        #region members

        protected const string UndefTag        = "undef";
        protected const string UnknownDataType = "(unknown)";

        // set to null if we're just using the value
        public readonly string Key;
        public string DataType = UnknownDataType;

        #endregion

        // constructor
        protected AbstractData() { }

        // constructor
        protected AbstractData(string key)
        {
            Key = key;
        }

        /// <summary>
        /// returns a string representation of our object
        /// </summary>
        public override string ToString()
        {
            return DumpData(0, true);
        }

        /// <summary>
        /// returns the object value if it exists, null otherwise
        /// </summary>
        internal abstract ObjectValue SearchObjectValue(string[] searchKeys, int currentKeyIndex);

        /// <summary>
        /// returns the object value array if it exists, null otherwise
        /// </summary>
        internal abstract List<AbstractData> SearchObjectValues(string[] searchKeys, int currentKeyIndex);

        /// <summary>
        /// returns the string if it exists, null otherwise
        /// </summary>
        internal abstract string Search(string[] searchKeys, int currentKeyIndex);

        /// <summary>
        /// dumps the data contained within this value
        /// </summary>
        internal abstract string DumpData(int indentLen, bool isLastChild);
    }
}
