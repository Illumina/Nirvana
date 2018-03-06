using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Import;

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

        /// <summary>
        /// parses the relevant data from each protein function predictions object
        /// </summary>
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
                    case ImportKeys.PolyPhenHumDiv:
                        // not used
                        break;
                    case ImportKeys.PolyPhen:
                        if (node.IsUndefined())
                        {
                            // do nothing
                        }
                        else
                        {
                            throw new InvalidDataException($"Could not handle the PolyPhen key: [{node.GetType()}]");
                        }
                        break;
                    case ImportKeys.PolyPhenHumVar:
                        // used by default
                        if (node is ObjectKeyValueNode polyPhenHumVarNode)
                        {
                            polyphenData = ImportPrediction.Parse(polyPhenHumVarNode.Value);
                        }
                        else if (!node.IsUndefined())
                        {
                            throw new InvalidDataException($"Could not transform the AbstractData object into an ObjectKeyValue: [{node.GetType()}]");
                        }
                        break;
                    case ImportKeys.Sift:
                        if (node is ObjectKeyValueNode siftNode)
                        {
                            siftData = ImportPrediction.Parse(siftNode.Value);
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

            return (siftData, polyphenData);
        }
    }
}
