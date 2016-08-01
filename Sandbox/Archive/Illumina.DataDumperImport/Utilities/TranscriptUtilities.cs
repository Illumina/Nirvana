using System;
using System.Collections.Generic;
using Illumina.DataDumperImport.DataStructures;
using Illumina.DataDumperImport.DataStructures.VEP;
using Illumina.VariantAnnotation.DataStructures;

namespace Illumina.DataDumperImport.Utilities
{
    public static class TranscriptUtilities
    {
        #region members

        private const string CodingDnaMapperUnitTypeKey = "cdna";
        private const string GenomeMapperUnitTypeKey    = "genome";

        private static readonly Dictionary<string, MapperUnitType> MapperUnitTypes;

        #endregion

        // constructor
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
        public static BioType GetBiotype(AbstractData ad)
        {
            return BioTypeUtilities.GetBiotypeFromString(DumperUtilities.GetString(ad));
        }

        /// <summary>
        /// returns the biotype given the specialized string key/value type
        /// </summary>
        public static GeneSymbolSource GetGeneSymbolSource(AbstractData ad)
        {
            return GeneSymbolSourceUtilities.GetGeneSymbolSourceFromString(DumperUtilities.GetString(ad));
        }

        /// <summary>
        /// returns the mapper unit type given the specialized string key/value type
        /// </summary>
        public static MapperUnitType GetMapperUnitType(AbstractData ad)
        {
            string mapperUnitTypeString = DumperUtilities.GetString(ad);

            MapperUnitType ret;
            if (!MapperUnitTypes.TryGetValue(mapperUnitTypeString, out ret))
            {
                throw new ApplicationException(
                    $"Unable to find the specified mapper unit type ({mapperUnitTypeString}) in the MapperUnitType dictionary.");
            }

            return ret;
        }

        /// <summary>
        /// returns true if the annotation is on the reverse strand, false otherwise
        /// </summary>
        public static bool GetStrand(AbstractData ad)
        {
            int strandNum = DumperUtilities.GetInt32(ad);

            // sanity check: make sure the value is either 1 or -1
            if ((strandNum != -1) && (strandNum != 1))
            {
                throw new ApplicationException($"Expected the strand number to be either -1 or 1. Found: {strandNum}.");
            }

            return strandNum == -1;
        }
    }
}
