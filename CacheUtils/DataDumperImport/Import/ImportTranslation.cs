using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Import;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CacheUtils.DataDumperImport.Utilities;
using Genome;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class ImportTranslation
    {
        private static readonly HashSet<string> KnownKeys;

        static ImportTranslation()
        {
            KnownKeys = new HashSet<string>
            {
                ImportKeys.Adaptor,
                ImportKeys.DbId,
                ImportKeys.EndExon,
                ImportKeys.End,
                ImportKeys.Sequence,
                ImportKeys.StableId,
                ImportKeys.StartExon,
                ImportKeys.Start,
                ImportKeys.Transcript,
                ImportKeys.Version
            };
        }

        /// <summary>
        /// parses the relevant data from each translation object
        /// </summary>
        public static (int Start, int End, string ProteinId, byte ProteinVersion, MutableExon startExon, MutableExon
            endExon) Parse(IImportNode importNode, IChromosome currentChromosome)
        {
            var objectValue = importNode.GetObjectValueNode();
            if (objectValue == null) throw new InvalidDataException("Encountered a translation import node that could not be converted to an object value node.");

            int start             = -1;
            int end               = -1;
            string proteinId      = null;
            byte proteinVersion   = 0;
            MutableExon startExon = null;
            MutableExon endExon   = null;

            foreach (var node in objectValue.Values)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(node.Key))
                {
                    throw new InvalidDataException($"Encountered an unknown key in the dumper mapper object: {node.Key}");
                }

                ObjectKeyValueNode exonNode;

                switch (node.Key)
                {
                    case ImportKeys.Adaptor:
                    case ImportKeys.Sequence:
                    case ImportKeys.DbId:
                    case ImportKeys.Transcript:
                        // skip this key
                        break;
                    case ImportKeys.StartExon:
                        exonNode = node as ObjectKeyValueNode;
                        if (exonNode != null) startExon = ImportExon.Parse(exonNode.Value, currentChromosome);
                        break;
                    case ImportKeys.EndExon:
                        exonNode = node as ObjectKeyValueNode;
                        if (exonNode != null) endExon = ImportExon.Parse(exonNode.Value, currentChromosome);
                        break;
                    case ImportKeys.StableId:
                        proteinId = node.GetString();
                        break;
                    case ImportKeys.End:
                        end = node.GetInt32();
                        break;
                    case ImportKeys.Start:
                        start = node.GetInt32();
                        break;
                    case ImportKeys.Version:
                        proteinVersion = (byte)node.GetInt32();
                        break;
                    default:
                        throw new InvalidDataException($"Unknown key found: {node.Key}");
                }
            }

            return (start, end, proteinId, proteinVersion, startExon, endExon);
        }
    }
}
