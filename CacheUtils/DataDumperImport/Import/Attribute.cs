using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CacheUtils.DataDumperImport.DataStructures.Import;
using CacheUtils.DataDumperImport.Utilities;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class Attribute
    {
        private static readonly HashSet<string> KnownKeys;
        private static readonly Regex RangeRegex;

        static Attribute()
        {
            KnownKeys = new HashSet<string>
            {
                ImportKeys.Name,
                ImportKeys.Description,
                ImportKeys.Code,
                ImportKeys.Value
            };

            RangeRegex = new Regex("(\\d+)-(\\d+)", RegexOptions.Compiled);
        }

        /// <summary>
        /// returns an array of miRNAs given a list of ObjectValues (AbstractData)
        /// </summary>
        public static (IInterval[] MicroRnas, IRnaEdit[] RnaEdits, bool CdsStartNotFound, bool CdsEndNotFound) ParseList(
            IImportNode importNode)
        {
            var listMembers = importNode.GetListMembers();
            if (listMembers == null) throw new InvalidDataException("Encountered an attribute node that could not be converted to a member list.");

            var microRnaList     = new List<IInterval>();
            var rnaEditList      = new List<IRnaEdit>();
            var cdsStartNotFound = false;
            var cdsEndNotFound   = false;

            foreach (var node in listMembers)
            {
                if (!(node is ObjectValueNode objectValue))
                    throw new InvalidDataException($"Could not transform the AbstractData object into an ObjectValue: [{node.GetType()}]");

                (var key, var value) = ParseKeyValue(objectValue);
                if (key == null) continue;

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (key)
                {
                    case "miRNA":
                        microRnaList.Add(GetInterval(value));
                        break;
                    case "_rna_edit":
                        rnaEditList.Add(GetRnaEdit(value));
                        break;
                    case "cds_start_NF":
                        cdsStartNotFound = true;
                        break;
                    case "cds_end_NF":
                        cdsEndNotFound = true;
                        break;
                }
            }

            var microRnas = microRnaList.Count == 0 ? null : microRnaList.ToArray();
            var rnaEdits  = rnaEditList.Count  == 0 ? null : rnaEditList.ToArray();
            return (microRnas, rnaEdits, cdsStartNotFound, cdsEndNotFound);
        }

        private static IInterval GetInterval(string s)
        {
            var rangeMatch = RangeRegex.Match(s);
            if (!rangeMatch.Success) throw new InvalidDataException($"Unable to convert the Attribute to a miRNA object. The value string failed the regex: {s}");

            int start = int.Parse(rangeMatch.Groups[1].Value);
            int end   = int.Parse(rangeMatch.Groups[2].Value);

            return new Interval(start, end);
        }

        private static RnaEdit GetRnaEdit(string s)
        {
            var cols = s.Split(' ');
            if (cols.Length != 3) throw new InvalidDataException($"Expected 3 columns but found {cols.Length} when parsing RNA edit");

            int start    = int.Parse(cols[0]);
            int end      = int.Parse(cols[1]);
            string bases = cols[2];

            return new RnaEdit(start, end, bases);
        }

        private static (string Key, string Value) ParseKeyValue(ObjectValueNode objectValue)
        {
            string key   = null;
            string value = null;

            foreach (var node in objectValue.Values)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(node.Key))
                {
                    throw new InvalidDataException($"Encountered an unknown key in the dumper attribute object: {node.Key}");
                }

                switch (node.Key)
                {
                    case ImportKeys.Name:
                    case ImportKeys.Description:
                        // not used
                        break;
                    case ImportKeys.Code:
                        key = node.GetString();
                        break;
                    case ImportKeys.Value:
                        value = node.GetString();
                        break;
                    default:
                        throw new InvalidDataException($"Unknown key found: {node.Key}");
                }
            }

            return (key, value);
        }
    }
}
