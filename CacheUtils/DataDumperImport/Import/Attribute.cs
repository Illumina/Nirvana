using System.Collections.Generic;
using System.Text.RegularExpressions;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures.Intervals;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class Attribute
    {
        #region members

        private const string NameKey        = "name";
        private const string DescriptionKey = "description";
        private const string CodeKey        = "code";
        private const string ValueKey       = "value";

        private static readonly HashSet<string> KnownKeys;
        private static readonly Regex RangeRegex;

        #endregion

        // constructor
        static Attribute()
        {
            KnownKeys = new HashSet<string>
            {
                NameKey,
                DescriptionKey,
                CodeKey,
                ValueKey
            };

            RangeRegex = new Regex("(\\d+)-(\\d+)", RegexOptions.Compiled);
        }

        /// <summary>
        /// returns an array of miRNAs given a list of ObjectValues (AbstractData)
        /// </summary>
        public static SimpleInterval[] ParseList(List<AbstractData> abstractDataList)
        {
            var microRnas = new List<SimpleInterval>();

            foreach (var ad in abstractDataList)
            {
                // skip references
                if (DumperUtilities.IsReference(ad)) continue;

                var objectValue = ad as ObjectValue;

                if (objectValue != null)
                {
                    var newMicroRna = Parse(objectValue);
                    if (newMicroRna != null)
                    {
                        microRnas.Add(newMicroRna);
                    }
                }
                else
                {
                    throw new GeneralException($"Could not transform the AbstractData object into an ObjectValue: [{ad.GetType()}]");
                }
            }

            return microRnas.Count == 0 ? null : microRnas.ToArray();
        }

        /// <summary>
        /// parses the relevant data from each attribute
        /// </summary>
        private static SimpleInterval Parse(ObjectValue objectValue)
        {
            string key   = null;
            string value = null;

            // loop over all of the key/value pairs in the gene object
            foreach (AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new GeneralException(
                        $"Encountered an unknown key in the dumper attribute object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case NameKey:
                    case DescriptionKey:
                        // not used
                        break;
                    case CodeKey:
                        key = DumperUtilities.GetString(ad);
                        break;
                    case ValueKey:
                        value = DumperUtilities.GetString(ad);
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            // sanity check: make sure this is a miRNA object
            if (key != "miRNA" || value == null)
            {
                return null;
            }

            var rangeMatch = RangeRegex.Match(value);

            if (!rangeMatch.Success)
            {
                throw new GeneralException("Unable to convert the Attribute to a miRNA object. The value string failed the regex: " + value);
            }

            int start = int.Parse(rangeMatch.Groups[1].Value);
            int end   = int.Parse(rangeMatch.Groups[2].Value);

            return new SimpleInterval(start, end);
        }
    }
}
