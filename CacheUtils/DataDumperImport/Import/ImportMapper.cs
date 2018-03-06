using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Import;
using CacheUtils.DataDumperImport.DataStructures.Mutable;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class ImportMapper
    {
        private static readonly HashSet<string> KnownKeys;

        static ImportMapper()
        {
            KnownKeys = new HashSet<string>
            {
                ImportKeys.FromCoordSystem,
                ImportKeys.FromName,
                ImportKeys.IsSorted,
                ImportKeys.PairCodingDna,
                ImportKeys.PairCount,
                ImportKeys.PairGenomic,
                ImportKeys.ToCoordSystem,
                ImportKeys.ToName
            };
        }

        /// <summary>
        /// parses the relevant data from each exon coordinate mapper object
        /// </summary>
        public static MutableTranscriptRegion[] Parse(ObjectValueNode objectValue)
        {
            MutableTranscriptRegion[] cdnaMaps = null;

            foreach (var node in objectValue.Values)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(node.Key))
                {
                    throw new InvalidDataException($"Encountered an unknown key in the dumper mapper object: {node.Key}");
                }

                switch (node.Key)
                {
                    case ImportKeys.ToName:
                    case ImportKeys.PairCount:
                    case ImportKeys.PairCodingDna:
                    case ImportKeys.FromCoordSystem:
                    case ImportKeys.FromName:
                    case ImportKeys.IsSorted:
                    case ImportKeys.ToCoordSystem:
                        // not used
                        break;
                    case ImportKeys.PairGenomic:
                        if (node is ObjectKeyValueNode pairGenomicNode)
                        {
                            cdnaMaps = ImportPairGenomic.Parse(pairGenomicNode.Value);
                        }
                        else if (!node.IsUndefined())
                        {
                            throw new InvalidDataException($"Could not transform the AbstractData object into an ObjectKeyValue: [{node.GetType()}]");
                        }
                        break;
                    default:
                        throw new InvalidDataException($"Unknown key found: {node.Key}");
                }
            }

            return cdnaMaps;
        }
    }
}
