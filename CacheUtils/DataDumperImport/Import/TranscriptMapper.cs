using System.Collections.Generic;
using CacheUtils.DataDumperImport.DataStructures;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
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
        public static DataStructures.VEP.TranscriptMapper Parse(ObjectValue objectValue, ImportDataStore dataStore)
        {
            var mapper = new DataStructures.VEP.TranscriptMapper();

            // loop over all of the key/value pairs in the transcript mapper object
            foreach (AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new GeneralException(
                        $"Encountered an unknown key in the dumper transcript mapper object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case CodingDnaCodingEndKey:
                    case CodingDnaCodingStartKey:
                    case StartPhaseKey:
                        break;
                    case ExonCoordinateMapperKey:
                        var exonCoordMapperNode = ad as ObjectKeyValue;
                        if (exonCoordMapperNode != null)
                        {
                            mapper.ExonCoordinateMapper = Mapper.Parse(exonCoordMapperNode.Value, dataStore);
                        }
                        else
                        {
                            throw new GeneralException(
                                $"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            return mapper;
        }

        /// <summary>
        /// parses the relevant data from each transcript mapper cache
        /// </summary>
        public static void ParseReference(ObjectValue objectValue, DataStructures.VEP.TranscriptMapper transcriptMapper, ImportDataStore dataStore)
        {
            // loop over all of the key/value pairs in the transcript mapper object
            foreach (AbstractData ad in objectValue)
            {
                if (ad.Key != ExonCoordinateMapperKey) continue;

                var exonMapperNode = ad as ObjectKeyValue;
                if (exonMapperNode != null) Mapper.ParseReference(exonMapperNode.Value, transcriptMapper.ExonCoordinateMapper, dataStore);
            }
        }
    }
}
