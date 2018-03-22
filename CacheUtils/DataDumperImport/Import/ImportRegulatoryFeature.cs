using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Import;
using CacheUtils.Helpers;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Sequence;

namespace CacheUtils.DataDumperImport.Import
{
    public static class ImportRegulatoryFeature
    {
        private static readonly HashSet<string> KnownKeys;

        static ImportRegulatoryFeature()
        {
            KnownKeys = new HashSet<string>
            {
                ImportKeys.AnalysisId,
                ImportKeys.BoundLengths,
                ImportKeys.CellTypeCount,
                ImportKeys.CellTypes,
                ImportKeys.DbId,
                ImportKeys.DisplayLabel,
                ImportKeys.End,
                ImportKeys.EpigenomeCount,
                ImportKeys.FeatureType,
                ImportKeys.HasEvidence,
                ImportKeys.Projected,
                ImportKeys.RegulatoryBuildId,
                ImportKeys.Set,
                ImportKeys.StableId,
                ImportKeys.Start,
                ImportKeys.Strand,
                ImportKeys.Slice,
                ImportKeys.VepFeatureType
            };
        }

        /// <summary>
        /// parses the relevant data from each regulatory element
        /// </summary>
        public static IRegulatoryRegion Parse(ObjectValueNode objectValue, IChromosome chromosome)
        {
            int start       = -1;
            int end         = -1;
            string stableId = null;
            string type     = null;

            foreach (var node in objectValue.Values)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(node.Key))
                {
                    throw new InvalidDataException($"Encountered an unknown key in the dumper regulatory element object: {node.Key}");
                }

                switch (node.Key)
                {
                    case ImportKeys.AnalysisId:
                    case ImportKeys.BoundLengths:
                    case ImportKeys.CellTypeCount:
                    case ImportKeys.CellTypes:
                    case ImportKeys.DbId:
                    case ImportKeys.DisplayLabel:
                    case ImportKeys.EpigenomeCount:
                    case ImportKeys.HasEvidence:
                    case ImportKeys.Projected:
                    case ImportKeys.RegulatoryBuildId:
                    case ImportKeys.Set:
                    case ImportKeys.Strand:
                    case ImportKeys.Slice:
                    case ImportKeys.VepFeatureType:
                        // not used
                        break;
                    case ImportKeys.FeatureType:
                        type = node.GetString();
                        break;
                    case ImportKeys.End:
                        end = node.GetInt32();
                        break;
                    case ImportKeys.StableId:
                        stableId = node.GetString();
                        break;
                    case ImportKeys.Start:
                        start = node.GetInt32();
                        break;
                    default:
                        throw new InvalidDataException($"Unknown key found: {node.Key}");
                }
            }

            return new RegulatoryRegion(chromosome, start, end, CompactId.Convert(stableId),
                RegulatoryRegionTypeHelper.GetRegulatoryRegionType(type));
        }
    }
}
