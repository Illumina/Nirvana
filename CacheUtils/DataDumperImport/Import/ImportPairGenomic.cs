using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Import;
using CacheUtils.DataDumperImport.DataStructures.Mutable;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class ImportPairGenomic
    {
        private static readonly HashSet<string> KnownKeys;

        static ImportPairGenomic()
        {
            KnownKeys = new HashSet<string>
            {
                ImportKeys.Genomic
            };
        }

        /// <summary>
        /// parses the relevant data from each pair genomic object
        /// </summary>
        public static MutableTranscriptRegion[] Parse(ObjectValueNode objectValue)
        {
            MutableTranscriptRegion[] cdnaMaps = null;

            foreach (var node in objectValue.Values)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(node.Key))
                {
                    throw new InvalidDataException($"Encountered an unknown key in the pair genomic object: {node.Key}");
                }

                switch (node.Key)
                {
                    case ImportKeys.Genomic:
                        if (node is ListObjectKeyValueNode genomicNode)
                        {
                            cdnaMaps = ImportMapperPair.ParseList(genomicNode.Values);
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
