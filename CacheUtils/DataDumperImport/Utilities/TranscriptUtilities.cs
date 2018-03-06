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

        /// <summary>
        /// returns the biotype given the specialized string key/value type
        /// </summary>
        public static BioType GetBiotype(IImportNode node) => BioTypeHelper.GetBioType(node.GetString());

        /// <summary>
        /// returns the mapper unit type given the specialized string key/value type
        /// </summary>
        public static MapperUnitType GetMapperUnitType(IImportNode node)
        {
            string mapperUnitTypeString = node.GetString();

            if (!MapperUnitTypes.TryGetValue(mapperUnitTypeString, out var ret))
            {
                throw new InvalidDataException($"Unable to find the specified mapper unit type ({mapperUnitTypeString}) in the MapperUnitType dictionary.");
            }

            return ret;
        }

        /// <summary>
        /// returns true if the annotation is on the reverse strand, false otherwise
        /// </summary>
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
    }
}
