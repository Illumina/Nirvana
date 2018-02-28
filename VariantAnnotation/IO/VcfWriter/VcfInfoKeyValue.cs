using System;
using System.Collections.Generic;

namespace VariantAnnotation.IO.VcfWriter
{
    public sealed class VcfInfoKeyValue
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
            _entries = new List<string>();
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
        /// used to update the non array info field. we initiate the info with empty emtry for every allele and update them if new value provided from supplelemtary annotation.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="genotypeIndex"></param>
        public void Add(string s, int genotypeIndex)
        {
            if (_entries.Count < genotypeIndex)
            {
                throw new ArgumentOutOfRangeException("GenotypeIndex for vcf info field is larger than the total number of alleles");
            }
            if (genotypeIndex <= 0)
            {
                throw new NotSupportedException("No info field should be written for reference allele");
            }
            if (s != null)
            {
                _entries[genotypeIndex - 1] = s;
                _containsNonEmptyEntries = true;
            }
        }

        /// <summary>
        /// returns a string representation of the VCF info field
        /// </summary>
        public string GetString()
        {
            if (!_containsNonEmptyEntries) return null;
            if (_infoFieldName == "AA")
            {
                return _infoFieldName + "=" + _entries[0];
            }

            return _infoFieldName + "=" + string.Join(",", _entries);
        }
    }
}