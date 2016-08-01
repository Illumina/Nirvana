using System.Collections.Generic;

namespace VariantAnnotation.DataStructures
{
    public class VcfInfoKeyValue
    {
        #region members

        private readonly List<string> _entries;
        private bool _containsNonEmptyEntries;
        private readonly string _infoFieldName;

        public const string VcfEmptyEntry = ".";

        #endregion

        // constructor
        public VcfInfoKeyValue(string infoFieldName)
        {
            _infoFieldName = infoFieldName;
            _entries       = new List<string>();
        }

        public void Add(string s)
        {
            if (s != null)
            {
                _entries.Add(s);
                _containsNonEmptyEntries = true;
            }
            else
            {
                _entries.Add(VcfEmptyEntry);
            }
        }

        /// <summary>
        /// returns a string representation of the VCF info field
        /// </summary>
        public string GetString()
        {
            if (!_containsNonEmptyEntries) return null;
            return _infoFieldName + "=" + string.Join(",", _entries);
        }
    }
}
