using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Import;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class ImportSeqEdits
    {
        private static readonly HashSet<string> KnownKeys;

        static ImportSeqEdits()
        {
            KnownKeys = new HashSet<string>
            {
                ImportKeys.AltSeq,
                ImportKeys.Code,
                ImportKeys.Description,
                ImportKeys.End,
                ImportKeys.Name,
                ImportKeys.Start
            };
        }

        /// <summary>
        /// parses the relevant data from each seqedits object
        /// </summary>
        public static int[] Parse(List<IListMember> members)
        {
            var selenocysteineList = new List<int>();

            foreach (var seqEditNode in members)
            {
                if (!(seqEditNode is ObjectValueNode seListNode)) continue;

                string code = null;
                int start   = -1;

                foreach (var node in seListNode.Values)
                {
                    // sanity check: make sure we know about the keys are used for
                    if (!KnownKeys.Contains(node.Key))
                    {
                        throw new InvalidDataException($"Encountered an unknown key in the dumper seq_edits object: {node.Key}");
                    }

                    switch (node.Key)
                    {
                        case ImportKeys.AltSeq:
                        case ImportKeys.Description:
                        case ImportKeys.End:
                        case ImportKeys.Name:
                            // not used
                            break;
                        case ImportKeys.Code:
                            code = node.GetString();
                            break;
                        case ImportKeys.Start:
                            start = node.GetInt32();
                            break;
                        default:
                            throw new InvalidDataException($"Unknown key found: {node.Key}");
                    }
                }

                if (code != null && code == "_selenocysteine") selenocysteineList.Add(start);
            }

            return selenocysteineList.Count == 0 ? null : selenocysteineList.ToArray();
        }
    }
}
