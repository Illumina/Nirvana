using System.Collections.Generic;
using CacheUtils.DataDumperImport.DataStructures;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class CoordSystem
    {
        #region members

        private const string DefaultKey       = "default";
        private const string NameKey          = "name";
        private const string RankKey          = "rank";
        private const string SequenceLevelKey = "sequence_level";
        private const string TopLevelKey      = "top_level";

        private static readonly HashSet<string> KnownCoordinateSystemKeys;

        #endregion

        // constructor
        static CoordSystem()
        {
            KnownCoordinateSystemKeys = new HashSet<string>
            {
                Transcript.DbIdKey,
                DefaultKey,
                NameKey,
                RankKey,
                SequenceLevelKey,
                TopLevelKey,
                Transcript.VersionKey
            };
        }

        /// <summary>
        /// returns a new coordinate system given an ObjectValue
        /// </summary>
        public static DataStructures.VEP.CoordSystem Parse(ObjectValue objectValue)
        {
            foreach (AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownCoordinateSystemKeys.Contains(ad.Key))
                {
                    throw new GeneralException($"Encountered an unknown key in the dumper mapper object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case Transcript.DbIdKey:
                    case DefaultKey:
                    case NameKey:
                    case RankKey:
                    case SequenceLevelKey:
                    case TopLevelKey:
                    case Transcript.VersionKey:
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            return new DataStructures.VEP.CoordSystem();
        }
    }
}
