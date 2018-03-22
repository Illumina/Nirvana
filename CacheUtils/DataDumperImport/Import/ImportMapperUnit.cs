using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Import;
using CacheUtils.DataDumperImport.Utilities;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class ImportMapperUnit
    {
        private static readonly HashSet<string> KnownKeys;

        static ImportMapperUnit()
        {
            KnownKeys = new HashSet<string>
            {
                ImportKeys.End,
                ImportKeys.Id,
                ImportKeys.Start
            };
        }

        /// <summary>
        /// parses the relevant data from each mapper unit object
        /// </summary>
        public static (int Start, int End, MapperUnitType Type) Parse(ObjectValueNode objectValue)
        {
            int start = -1;
            int end   = -1;
            var type  = MapperUnitType.Unknown;

            foreach (var node in objectValue.Values)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(node.Key))
                {
                    throw new InvalidDataException($"Encountered an unknown key in the mapper unit object: {node.Key}");
                }

                switch (node.Key)
                {
                    case ImportKeys.Id:
                        type = TranscriptUtilities.GetMapperUnitType(node);
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

            return (start, end, type);
        }
    }

    public enum MapperUnitType : byte
    {
        Unknown,
        CodingDna,
        Genomic
    }
}
