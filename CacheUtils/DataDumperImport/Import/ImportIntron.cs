using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Import;
using VariantAnnotation.Interface.Intervals;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class ImportIntron
    {
        private static readonly HashSet<string> KnownKeys;

        static ImportIntron()
        {
            KnownKeys = new HashSet<string>
            {
                ImportKeys.Analysis,
                ImportKeys.Adaptor,
                ImportKeys.DbId,
                ImportKeys.End,
                ImportKeys.Next,
                ImportKeys.Prev,
                ImportKeys.SeqName,
                ImportKeys.Slice,
                ImportKeys.Start,
                ImportKeys.Strand
            };
        }

        /// <summary>
        /// returns a new exon given an ObjectValue
        /// </summary>
        private static IInterval Parse(ObjectValueNode objectValue)
        {
            int start = -1;
            int end   = -1;

            foreach (var node in objectValue.Values)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(node.Key))
                {
                    throw new InvalidDataException($"Encountered an unknown key in the dumper mapper object: {node.Key}");
                }

                switch (node.Key)
                {
                    case ImportKeys.Analysis:
                    case ImportKeys.Adaptor:
                    case ImportKeys.DbId:
                    case ImportKeys.Next:
                    case ImportKeys.Prev:
                    case ImportKeys.SeqName:
                    case ImportKeys.Strand:
                    case ImportKeys.Slice:
                        // not used
                        break;
                    case ImportKeys.End:
                        end = node.GetInt32();
                        break;
                    case ImportKeys.Start:
                        start = node.GetInt32();
                        break;
                    default:
                        throw new InvalidDataException($"Unknown key found: {node.Key}");
                }
            }

            return new Interval(start, end);
        }

        /// <summary>
        /// parses the relevant data from each intron object
        /// </summary>
        public static IInterval[] ParseList(List<IListMember> members)
        {
            var introns = new IInterval[members.Count];

            for (var intronIndex = 0; intronIndex < members.Count; intronIndex++)
            {
                if (!(members[intronIndex] is ObjectValueNode objectValue))
                {
                    throw new InvalidDataException($"Could not transform the AbstractData object into an ObjectValue: [{members[intronIndex].GetType()}]");
                }

                introns[intronIndex] = Parse(objectValue);
            }

            return introns;
        }
    }
}
