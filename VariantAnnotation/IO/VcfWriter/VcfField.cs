using System.Collections.Generic;

namespace VariantAnnotation.IO.VcfWriter
{
    public sealed class VcfField
    {
        #region members

        private readonly HashSet<string> _entries;

        #endregion

        // constructor
        public VcfField()
        {
            _entries = new HashSet<string>();
        }

        public void Add(string s)
        {
            if (s == null) return;
            _entries.Add(s);
        }


        /// <summary>
        /// returns a string representation of the VCF ID field
        /// </summary>
        public string GetString(string previousEntries)
        {
            if (_entries.Count == 0)
            {
                return string.IsNullOrEmpty(previousEntries) ? VcfInfoKeyValue.VcfEmptyEntry : previousEntries;
            }

            var s = string.Join(";", _entries);

            if (string.IsNullOrEmpty(previousEntries)) return s;
            return previousEntries + ";" + s;
        }
    }
}