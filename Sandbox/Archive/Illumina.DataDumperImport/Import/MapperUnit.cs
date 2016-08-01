using System;
using System.Collections.Generic;
using Illumina.DataDumperImport.Utilities;
using DS = Illumina.DataDumperImport.DataStructures;

namespace Illumina.DataDumperImport.Import
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
        public static DS.VEP.MapperUnit Parse(DS.ObjectValue objectValue, ushort currentReferenceIndex)
        {
            var id    = DS.VEP.MapperUnitType.Unknown;
            int end   = -1;
            int start = -1;

            // loop over all of the key/value pairs in the mapper unit object
            foreach (DS.AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new ApplicationException($"Encountered an unknown key in the mapper unit object: {ad.Key}");
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
                        throw new ApplicationException($"Unknown key found: {ad.Key}");
                }
            }

            return new DS.VEP.MapperUnit(currentReferenceIndex, start, end, id);
        }
    }
}
