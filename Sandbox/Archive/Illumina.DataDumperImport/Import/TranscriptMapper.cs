using System;
using System.Collections.Generic;
using Illumina.DataDumperImport.Utilities;
using DS = Illumina.DataDumperImport.DataStructures;

namespace Illumina.DataDumperImport.Import
{
    internal static class TranscriptMapper
    {
        #region members

        private const string CodingDnaCodingEndKey   = "cdna_coding_end";
        private const string CodingDnaCodingStartKey = "cdna_coding_start";
        private const string ExonCoordinateMapperKey = "exon_coord_mapper";
        private const string StartPhaseKey           = "start_phase";

        private static readonly HashSet<string> KnownKeys;

        #endregion

        // constructor
        static TranscriptMapper()
        {
            KnownKeys = new HashSet<string>
            {
                CodingDnaCodingEndKey,
                CodingDnaCodingStartKey,
                ExonCoordinateMapperKey,
                StartPhaseKey
            };
        }

        /// <summary>
        /// parses the relevant data from each transcript mapper
        /// </summary>
        public static DS.VEP.TranscriptMapper Parse(DS.ObjectValue objectValue, DS.ImportDataStore dataStore)
        {
            var mapper = new DS.VEP.TranscriptMapper();

            // loop over all of the key/value pairs in the transcript mapper object
            foreach (DS.AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new ApplicationException(
                        $"Encountered an unknown key in the dumper transcript mapper object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case CodingDnaCodingEndKey:
                        DumperUtilities.GetInt32(ad);
                        break;
                    case CodingDnaCodingStartKey:
                        DumperUtilities.GetInt32(ad);
                        break;
                    case ExonCoordinateMapperKey:
                        var exonCoordMapperNode = ad as DS.ObjectKeyValue;
                        if (exonCoordMapperNode != null)
                        {
                            mapper.ExonCoordinateMapper = Mapper.Parse(exonCoordMapperNode.Value, dataStore);
                        }
                        else
                        {
                            throw new ApplicationException(
                                $"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    case StartPhaseKey:
                        DumperUtilities.GetInt32(ad);
                        break;
                    default:
                        throw new ApplicationException($"Unknown key found: {ad.Key}");
                }
            }

            return mapper;
        }

        /// <summary>
        /// parses the relevant data from each transcript mapper cache
        /// </summary>
        public static void ParseReference(DS.ObjectValue objectValue, DS.VEP.TranscriptMapper transcriptMapper, DS.ImportDataStore dataStore)
        {
            // loop over all of the key/value pairs in the transcript mapper object
            foreach (DS.AbstractData ad in objectValue)
            {
                if (ad.Key != ExonCoordinateMapperKey) continue;

                var exonMapperNode = ad as DS.ObjectKeyValue;
                if (exonMapperNode != null) Mapper.ParseReference(exonMapperNode.Value, transcriptMapper.ExonCoordinateMapper, dataStore);
            }
        }
    }
}
