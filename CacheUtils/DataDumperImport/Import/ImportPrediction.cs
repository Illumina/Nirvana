using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Import;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class ImportPrediction
    {
        private static readonly HashSet<string> KnownKeys;

        static ImportPrediction()
        {
            KnownKeys = new HashSet<string>
            {
                ImportKeys.Analysis,
                ImportKeys.IsMatrixCompressed,
                ImportKeys.Matrix,
                ImportKeys.PeptideLength,
                ImportKeys.SubAnalysis,
                ImportKeys.TranslationMd5
            };
        }

        /// <summary>
        /// parses the relevant data from each prediction object
        /// </summary>
        public static string Parse(ObjectValueNode objectValue)
        {
            string predictionData = null;

            foreach (var node in objectValue.Values)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(node.Key))
                {
                    throw new InvalidDataException($"Encountered an unknown key in the dumper prediction object: {node.Key}");
                }

                switch (node.Key)
                {
                    case ImportKeys.Analysis:
                    case ImportKeys.IsMatrixCompressed:
                    case ImportKeys.PeptideLength:
                    case ImportKeys.SubAnalysis:
                    case ImportKeys.TranslationMd5:
                        break;
                    case ImportKeys.Matrix:
                        predictionData = node.GetString();
                        break;
                    default:
                        throw new InvalidDataException($"Unknown key found: {node.Key}");
                }
            }

            return predictionData;
        }
    }
}
