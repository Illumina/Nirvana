using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Import;
using CacheUtils.DataDumperImport.DataStructures.Mutable;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class ImportTranscriptMapper
    {
        private static readonly HashSet<string> KnownKeys;

        static ImportTranscriptMapper()
        {
            KnownKeys = new HashSet<string>
            {
                ImportKeys.CodingDnaCodingEnd,
                ImportKeys.CodingDnaCodingStart,
                ImportKeys.ExonCoordinateMapper,
                ImportKeys.StartPhase
            };
        }

        /// <summary>
        /// parses the relevant data from each transcript mapper
        /// </summary>
        public static MutableTranscriptRegion[] Parse(ObjectValueNode objectValue)
        {
            MutableTranscriptRegion[] cdnaMaps = null;

            foreach (var node in objectValue.Values)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(node.Key))
                {
                    throw new InvalidDataException($"Encountered an unknown key in the dumper transcript mapper object: {node.Key}");
                }

                switch (node.Key)
                {
                    case ImportKeys.CodingDnaCodingEnd:
                    case ImportKeys.CodingDnaCodingStart:
                    case ImportKeys.StartPhase:
                        break;
                    case ImportKeys.ExonCoordinateMapper:
                        if (node is ObjectKeyValueNode exonCoordMapperNode)
                        {
                            cdnaMaps = ImportMapper.Parse(exonCoordMapperNode.Value);
                        }
                        else
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
