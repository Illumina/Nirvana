using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Import;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CacheUtils.DataDumperImport.Utilities;
using Genome;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class ImportExon
    {
        private static readonly HashSet<string> KnownKeys;

        static ImportExon()
        {
            KnownKeys = new HashSet<string>
            {
                ImportKeys.End,
                ImportKeys.EndPhase,
                ImportKeys.Phase,
                ImportKeys.StableId,
                ImportKeys.Start,
                ImportKeys.Strand
            };
        }

        /// <summary>
        /// returns a new exon given an ObjectValue
        /// </summary>
        public static MutableExon Parse(ObjectValueNode objectValue, IChromosome currentChromosome)
        {
            int start = -1;
            int end   = -1;
            int phase = int.MinValue;

            foreach (var node in objectValue.Values)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(node.Key))
                {
                    throw new InvalidDataException($"Encountered an unknown key in the dumper mapper object: {node.Key}");
                }

                switch (node.Key)
                {
                    case ImportKeys.Strand:
                    case ImportKeys.StableId:
                    case ImportKeys.EndPhase:
                        // not used
                        break;
                    case ImportKeys.End:
                        end = node.GetInt32();
                        break;
                    case ImportKeys.Phase:
                        phase = node.GetInt32();
                        break;
                    case ImportKeys.Start:
                        start = node.GetInt32();
                        break;
                    default:
                        throw new InvalidDataException($"Unknown key found: {node.Key}");
                }
            }

            return new MutableExon(currentChromosome, start, end, phase);
        }

        /// <summary>
        /// returns an array of exons given a list of ObjectValues (AbstractData)
        /// </summary>
        public static MutableExon[] ParseList(IImportNode importNode, IChromosome chromosome)
        {
            var listMembers = importNode.GetListMembers();
            if (listMembers == null) throw new InvalidDataException("Encountered an exon node that could not be converted to a member list.");

            var exons = new MutableExon[listMembers.Count];

            for (var exonIndex = 0; exonIndex < listMembers.Count; exonIndex++)
            {
                if (listMembers[exonIndex] is ObjectValueNode objectValue)
                {
                    exons[exonIndex] = Parse(objectValue, chromosome);
                }
                else
                {
                    throw new InvalidDataException($"Could not transform the AbstractData object into an ObjectValue: [{listMembers[exonIndex].GetType()}]");
                }
            }

            return exons;
        }
    }
}