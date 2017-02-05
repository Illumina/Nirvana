using System.Collections.Generic;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
{
    public static class RegulatoryFeature
    {
        #region members

        public const string DataType = "Bio::EnsEMBL::Funcgen::RegulatoryFeature";

        private const string ProjectedKey     = "projected";
        private const string SetKey           = "set";
        private const string CellTypesKey     = "cell_types";
        private const string DisplayLabelKey  = "display_label";
        private const string BoundLengthsKey  = "_bound_lengths";
        private const string DbIdKey          = "dbID";
        private const string FeatureTypeKey   = "feature_type";
        private const string CellTypeCountKey = "cell_type_count";
        private const string HasEvidenceKey   = "has_evidence";

        private static readonly HashSet<string> KnownKeys;

        #endregion

        // constructor
        static RegulatoryFeature()
        {
            KnownKeys = new HashSet<string>
            {
                BoundLengthsKey,
                CellTypeCountKey,
                CellTypesKey,
                DbIdKey,
                DisplayLabelKey,
                Transcript.EndKey,
                FeatureTypeKey,
                HasEvidenceKey,
                ProjectedKey,
                SetKey,
                Transcript.StableIdKey,
                Transcript.StartKey,
                Transcript.StrandKey,
                Transcript.SliceKey
            };
        }

        /// <summary>
        /// parses the relevant data from each regulatory element
        /// </summary>
        public static void Parse(ObjectValue objectValue, int regulatoryFeatureIndex, ImportDataStore dataStore)
        {
            // Console.WriteLine("*** Parse {0} ***", regulatoryFeatureIndex + 1);

            int start       = -1;
            int end         = -1;
            string stableId = null;
            string type     = null;

            // loop over all of the key/value pairs in the transcript object
            foreach (AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new GeneralException(
                        $"Encountered an unknown key in the dumper regulatory element object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case BoundLengthsKey:
                    case CellTypeCountKey:
                    case CellTypesKey:
                    case DbIdKey:
                    case DisplayLabelKey:
                    case HasEvidenceKey:
                    case ProjectedKey:
                    case SetKey:
                    case Transcript.StrandKey:
                    case Transcript.SliceKey:
                        // not used
                        break;
                    case FeatureTypeKey:
                        type = DumperUtilities.GetString(ad);
                        break;
                    case Transcript.EndKey:
                        end = DumperUtilities.GetInt32(ad);
                        break;
                    case Transcript.StableIdKey:
                        stableId = DumperUtilities.GetString(ad);
                        break;
                    case Transcript.StartKey:
                        start = DumperUtilities.GetInt32(ad);
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            dataStore.RegulatoryFeatures.Add(new DataStructures.VEP.RegulatoryFeature(dataStore.CurrentReferenceIndex, start, end, stableId, type));
        }
    }
}
