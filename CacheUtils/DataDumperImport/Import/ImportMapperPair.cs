using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Import;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class ImportMapperPair
    {
        private static readonly HashSet<string> KnownKeys;

        static ImportMapperPair()
        {
            KnownKeys = new HashSet<string>
            {
                ImportKeys.From,
                ImportKeys.Ori,
                ImportKeys.To
            };
        }

        /// <summary>
        /// parses the relevant data from each mapper pairs object
        /// </summary>
        private static MutableTranscriptRegion Parse(ObjectValueNode objectValue)
        {
            int fromStart = -1;
            int fromEnd   = -1;
            var fromType  = MapperUnitType.Unknown;
            int toStart   = -1;
            int toEnd     = -1;

            foreach (var node in objectValue.Values)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(node.Key))
                {
                    throw new InvalidDataException($"Encountered an unknown key in the mapper pair object: {node.Key}");
                }

                switch (node.Key)
                {
                    case ImportKeys.Ori:
                        // not used
                        break;
                    case ImportKeys.From:
                        if (node is ObjectKeyValueNode fromKeyNode)
                        {
                            (fromStart, fromEnd, fromType) = ImportMapperUnit.Parse(fromKeyNode.Value);
                        }
                        else
                        {
                            throw new InvalidDataException($"Could not transform the AbstractData object into an ObjectKeyValue: [{node.GetType()}]");
                        }
                        break;
                    case ImportKeys.To:
                        if (node is ObjectKeyValueNode toKeyNode)
                        {
                            (toStart, toEnd, _) = ImportMapperUnit.Parse(toKeyNode.Value);
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

            return GetCdnaMap(fromStart, fromEnd, fromType, toStart, toEnd);
        }

        private static MutableTranscriptRegion GetCdnaMap(int fromStart, int fromEnd, MapperUnitType fromType, int toStart, int toEnd)
        {
            return fromType == MapperUnitType.Genomic
                ? new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, fromStart, fromEnd, toStart, toEnd)
                : new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, toStart, toEnd, fromStart, fromEnd);
        }

        /// <summary>
        /// parses the relevant data from each mapper pairs object
        /// </summary>
        public static MutableTranscriptRegion[] ParseList(List<IListMember> listMembers)
        {
            var cdnaMaps = new List<MutableTranscriptRegion>(listMembers.Count);

            foreach (var entry in listMembers)
            {
                if (!(entry is ObjectValueNode mapperPairNode))          throw new InvalidDataException($"Could not transform the AbstractData object into an ObjectValue: [{entry.GetType()}]");
                if (mapperPairNode.Type != "Bio::EnsEMBL::Mapper::Pair") throw new InvalidDataException($"Expected a mapper pair data type, but found the following data type: [{mapperPairNode.Type}]");

                cdnaMaps.Add(Parse(mapperPairNode));
            }

            return cdnaMaps.ToArray();
        }
    }
}
