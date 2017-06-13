using System.Collections.Generic;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class Translation
    {
        #region members

        private const string AdaptorKey    = "adaptor";
        internal const string EndExonKey   = "end_exon";
        private const string SequenceKey   = "seq";
        internal const string StartExonKey = "start_exon";
        private const string TranscriptKey = "transcript";

        private static readonly HashSet<string> KnownKeys;

        #endregion

        // constructor
        static Translation()
        {
            KnownKeys = new HashSet<string>
            {
                AdaptorKey,
                Transcript.DbIdKey,
                EndExonKey,
                Transcript.EndKey,
                SequenceKey,
                Transcript.StableIdKey,
                StartExonKey,
                Transcript.StartKey,
                TranscriptKey,
                Transcript.VersionKey
            };
        }

        /// <summary>
        /// parses the relevant data from each translation object
        /// </summary>
        public static DataStructures.VEP.Translation Parse(ObjectValue objectValue, ImportDataStore dataStore)
        {
            var translation = new DataStructures.VEP.Translation();

            // loop over all of the key/value pairs in the translation object
            foreach (AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new GeneralException($"Encountered an unknown key in the dumper mapper object: {ad.Key}");
                }

                // handle each key
                ObjectKeyValue exonNode;
                switch (ad.Key)
                {
                    case AdaptorKey:
                    case SequenceKey:
                    case Transcript.DbIdKey:
                    case Transcript.StableIdKey:
                        // skip this key
                        break;
                    case EndExonKey:
                        exonNode = ad as ObjectKeyValue;
                        if (exonNode != null)
                        {
                            var newExon = Exon.Parse(exonNode.Value, dataStore.CurrentReferenceIndex);
                            translation.EndExon = newExon;
                        }
                        break;
                    case StartExonKey:
                        exonNode = ad as ObjectKeyValue;
                        if (exonNode != null)
                        {
                            var newExon = Exon.Parse(exonNode.Value, dataStore.CurrentReferenceIndex);
                            translation.StartExon = newExon;
                        }
                        break;
                    case TranscriptKey:
                        // parse this during the references
                        if (!DumperUtilities.IsReference(ad))
                        {
                            throw new GeneralException("Found a Translation->Transcript entry that wasn't a reference.");
                        }
                        break;
                    case Transcript.EndKey:
                        translation.End = DumperUtilities.GetInt32(ad);
                        break;
                    case Transcript.StartKey:
                        translation.Start = DumperUtilities.GetInt32(ad);
                        break;
                    case Transcript.VersionKey:
                        translation.Version = (byte)DumperUtilities.GetInt32(ad);
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            return translation;
        }

        /// <summary>
        /// points to a translation that has already been created
        /// </summary>
        public static void ParseReference(ObjectValue objectValue, DataStructures.VEP.Translation translation, ImportDataStore dataStore)
        {
            // loop over all of the key/value pairs in the translation object
            foreach (AbstractData ad in objectValue)
            {
                if (!DumperUtilities.IsReference(ad)) continue;

                // handle each key
                var referenceKeyValue = ad as ReferenceKeyValue;
                if (referenceKeyValue == null) continue;

                switch (referenceKeyValue.Key)
                {
                    case AdaptorKey:
                        // skip this key
                        break;
                    case EndExonKey:
                        translation.EndExon = Exon.ParseReference(referenceKeyValue.Value, dataStore);
                        break;
                    case StartExonKey:
                        translation.StartExon = Exon.ParseReference(referenceKeyValue.Value, dataStore);
                        break;
                    case TranscriptKey:
                        translation.Transcript = Transcript.ParseReference(referenceKeyValue.Value, dataStore);
                        break;
                    default:
                        throw new GeneralException(
                            $"Found an unhandled reference in the translation object: {ad.Key}");
                }
            }
        }
    }
}
