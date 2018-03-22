using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Import;
using CacheUtils.DataDumperImport.Utilities;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class ImportProteinFunctionPredictions
    {
        private static readonly HashSet<string> KnownKeys;

        static ImportProteinFunctionPredictions()
        {
            KnownKeys = new HashSet<string>
            {
                ImportKeys.PolyPhenHumVar,
                ImportKeys.PolyPhenHumDiv,
                ImportKeys.PolyPhen,
                ImportKeys.Sift
            };
        }

        public static (string SiftMatrix, string PolyphenMatrix) Parse(ObjectValueNode objectValue)
        {
            string siftData     = null;
            string polyphenData = null;

            foreach (var node in objectValue.Values)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(node.Key))
                {
                    throw new InvalidDataException($"Encountered an unknown key in the dumper mapper object: {node.Key}");
                }

                switch (node.Key)
                {
                    case ImportKeys.PolyPhen:
                    case ImportKeys.PolyPhenHumDiv:
                        // not used
                        break;
                    case ImportKeys.PolyPhenHumVar:
                        // used by default
                        polyphenData = node.GetPredictionData();
                        break;
                    case ImportKeys.Sift:
                        siftData = node.GetPredictionData();
                        break;
                    default:
                        throw new InvalidDataException($"Unknown key found: {node.Key}");
                }
            }

            return (siftData, polyphenData);
        }
    }
}
