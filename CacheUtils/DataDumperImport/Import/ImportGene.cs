using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Import;
using CacheUtils.DataDumperImport.Utilities;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class ImportGene
    {
        private static readonly HashSet<string> KnownKeys;

        static ImportGene()
        {
            KnownKeys = new HashSet<string>
            {
                ImportKeys.End,
                ImportKeys.StableId,
                ImportKeys.Start,
                ImportKeys.Strand
            };
        }

        public static (int Start, int End, string Id, bool OnReverseStrand) Parse(IImportNode importNode)
        {
            var objectValue = importNode.GetObjectValueNode();
            if (objectValue == null) throw new InvalidDataException("Encountered a gene import node that could not be converted to an object value node.");

            int start           = -1;
            int end             = -1;
            string stableId     = null;
            var onReverseStrand = false;

            foreach (var node in objectValue.Values)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(node.Key))
                {
                    throw new InvalidDataException($"Encountered an unknown key in the dumper gene object: {node.Key}");
                }

                // handle each key
                switch (node.Key)
                {
                    case ImportKeys.End:
                        end = node.GetInt32();
                        break;
                    case ImportKeys.StableId:
                        stableId = node.GetString();
                        break;
                    case ImportKeys.Start:
                        start = node.GetInt32();
                        break;
                    case ImportKeys.Strand:
                        onReverseStrand = TranscriptUtilities.GetStrand(node);
                        break;
                    default:
                        throw new InvalidDataException($"Unknown key found: {node.Key}");
                }
            }

            return (start, end, stableId, onReverseStrand);
        }
    }
}
