using System.Collections.Generic;
using ErrorHandling.Exceptions;

namespace VariantAnnotation.DataStructures
{
    public class VcfField
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
        /// given a vcf field (like the INFO field), this method returns a dictionary of the keys and values
        /// </summary>
        public static Dictionary<string, string> GetKeysAndValues(string vcfField)
        {
            var fields = new Dictionary<string, string>();

            foreach (var infoPair in vcfField.Split(';'))
            {
                var keyValuePair = infoPair.Split('=');

                // sanity check: make sure we only have one or two entries
                if ((keyValuePair.Length == 0) || (keyValuePair.Length > 2))
                {
                    throw new GeneralException($"Expected one or two entries in the key/value pair, but found {keyValuePair.Length} entries: {infoPair}");
                }

                fields.Add(keyValuePair[0], keyValuePair.Length == 2 ? keyValuePair[1] : null);
            }

            return fields;
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
