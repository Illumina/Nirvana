using System.Collections.Generic;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.DataStructures.VEP;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class MapperUnit
    {
        #region members

        private const string IdKey = "id";

        private static readonly HashSet<string> KnownKeys;

        #endregion

        // constructor
        static MapperUnit()
        {
            KnownKeys = new HashSet<string>
            {
                Transcript.EndKey,
                IdKey,
                Transcript.StartKey
            };
        }

        /// <summary>
        /// parses the relevant data from each mapper unit object
        /// </summary>
        public static DataStructures.VEP.MapperUnit Parse(ObjectValue objectValue, ushort currentReferenceIndex)
        {
            var id    = MapperUnitType.Unknown;
            int end   = -1;
            int start = -1;

            // loop over all of the key/value pairs in the mapper unit object
            foreach (AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new GeneralException($"Encountered an unknown key in the mapper unit object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case Transcript.EndKey:
                        end = DumperUtilities.GetInt32(ad);
                        break;
                    case IdKey:
                        id = TranscriptUtilities.GetMapperUnitType(ad);
                        break;
                    case Transcript.StartKey:
                        start = DumperUtilities.GetInt32(ad);
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            return new DataStructures.VEP.MapperUnit(currentReferenceIndex, start, end, id);
        }
    }
}
