using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Import;
using CacheUtils.DataDumperImport.Import;
using CacheUtils.Helpers;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.DataDumperImport.Utilities
{
    public static class TranscriptUtilities
    {
        private const string CodingDnaMapperUnitTypeKey = "cdna";
        private const string GenomeMapperUnitTypeKey    = "genome";

        private static readonly Dictionary<string, MapperUnitType> MapperUnitTypes;

        static TranscriptUtilities()
        {
            MapperUnitTypes = new Dictionary<string, MapperUnitType>
            {
                [CodingDnaMapperUnitTypeKey] = MapperUnitType.CodingDna,
                [GenomeMapperUnitTypeKey] = MapperUnitType.Genomic
            };
        }

        public static BioType GetBiotype(IImportNode node) => BioTypeHelper.GetBioType(node.GetString());

        public static MapperUnitType GetMapperUnitType(IImportNode node)
        {
            string mapperUnitTypeString = node.GetString();

            if (!MapperUnitTypes.TryGetValue(mapperUnitTypeString, out var ret))
            {
                throw new InvalidDataException($"Unable to find the specified mapper unit type ({mapperUnitTypeString}) in the MapperUnitType dictionary.");
            }

            return ret;
        }

        public static ObjectValueNode GetObjectValueNode(this IImportNode node)
        {
            if (node is ObjectKeyValueNode objectKeyValueNode) return objectKeyValueNode.Value;
            return null;
        }

        public static List<IListMember> GetListMembers(this IImportNode node)
        {
            if (node is ListObjectKeyValueNode listObjectKeyValueNode) return listObjectKeyValueNode.Values;
            return null;
        }

        public static bool GetStrand(IImportNode node)
        {
            int strandNum = node.GetInt32();

            // sanity check: make sure the value is either 1 or -1
            if (strandNum != -1 && strandNum != 1)
            {
                throw new InvalidDataException($"Expected the strand number to be either -1 or 1. Found: {strandNum}.");
            }

            return strandNum == -1;
        }

        public static int GetHgncId(this IImportNode node)
        {
            string hgnc = node.GetString();
            if (hgnc != null && hgnc.StartsWith("HGNC:")) hgnc = hgnc.Substring(5);

            int hgncId = -1;
            if (hgnc != null) hgncId = int.Parse(hgnc);
            return hgncId;
        }
    }
}
