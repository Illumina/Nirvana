using System;
using System.Collections.Generic;
using Illumina.DataDumperImport.Utilities;
using DS = Illumina.DataDumperImport.DataStructures;

namespace Illumina.DataDumperImport.Import
{
    internal static class Translation
    {
        #region members

        private const string AdaptorKey    = "adaptor";
        internal const string EndExonKey    = "end_exon";
        private const string SequenceKey   = "seq";
        internal const string StartExonKey  = "start_exon";
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
        public static DS.VEP.Translation Parse(DS.ObjectValue objectValue, DS.ImportDataStore dataStore)
        {
            var translation = new DS.VEP.Translation();

            // loop over all of the key/value pairs in the translation object
            foreach (DS.AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new ApplicationException($"Encountered an unknown key in the dumper mapper object: {ad.Key}");
                }

                // handle each key
                DS.ObjectKeyValue exonNode;
                switch (ad.Key)
                {
                    case AdaptorKey:
                        // skip this key
                        break;
                    case SequenceKey:
                        DumperUtilities.GetString(ad);
                        break;
                    case EndExonKey:
                        exonNode = ad as DS.ObjectKeyValue;
                        if (exonNode != null)
                        {
                            var newExon = Exon.Parse(exonNode.Value, dataStore.CurrentReferenceIndex);
                            // DS.VEP.Exon oldExon;
                            // if (dataStore.Exons.TryGetValue(newExon, out oldExon))
                            //{
                            //    translation.EndExon = oldExon;
                            //}
                            // else
                            //{
                            translation.EndExon = newExon;
                            //    dataStore.Exons[newExon] = newExon;
                            //}
                        }
                        break;
                    case StartExonKey:
                        exonNode = ad as DS.ObjectKeyValue;
                        if (exonNode != null)
                        {
                            var newExon = Exon.Parse(exonNode.Value, dataStore.CurrentReferenceIndex);
                            // DS.VEP.Exon oldExon;
                            // if (dataStore.Exons.TryGetValue(newExon, out oldExon))
                            //{
                            //    translation.StartExon = oldExon;
                            //}
                            // else
                            //{
                            translation.StartExon = newExon;
                            //    dataStore.Exons[newExon] = newExon;
                            //}
                        }
                        break;
                    case TranscriptKey:
                        // parse this during the references
                        if (!DumperUtilities.IsReference(ad))
                        {
                            throw new ApplicationException("Found a Translation->Transcript entry that wasn't a reference.");
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
                    case Transcript.DbIdKey:
                        DumperUtilities.GetString(ad);
                        break;
                    case Transcript.StableIdKey:
                        DumperUtilities.GetString(ad);
                        break;
                    default:
                        throw new ApplicationException($"Unknown key found: {ad.Key}");
                }
            }

            return translation;
        }

        /// <summary>
        /// points to a translation that has already been created
        /// </summary>
        public static void ParseReference(DS.ObjectValue objectValue, DS.VEP.Translation translation, DS.ImportDataStore dataStore)
        {
            // loop over all of the key/value pairs in the translation object
            foreach (DS.AbstractData ad in objectValue)
            {
                if (!DumperUtilities.IsReference(ad)) continue;

                // handle each key
                var referenceKeyValue = ad as DS.ReferenceKeyValue;
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
                        throw new ApplicationException(
                            $"Found an unhandled reference in the translation object: {ad.Key}");
                }
            }
        }
    }
}
