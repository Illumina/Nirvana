using System.Collections.Generic;
using System.Text.RegularExpressions;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class Exon
    {
        #region members

        private const string EndPhaseKey = "end_phase";
        private const string PhaseKey    = "phase";

        private static readonly HashSet<string> KnownKeys;

        private static readonly Regex SortedExonsReferenceRegex;
        private static readonly Regex TransExonArrayReferenceRegex;
        private static readonly Regex TranslationReferenceRegex;

        #endregion

        // constructor
        static Exon()
        {
            KnownKeys = new HashSet<string>
            {
                Transcript.EndKey,
                EndPhaseKey,
                PhaseKey,
                Transcript.StableIdKey,
                Transcript.StartKey,
                Transcript.StrandKey
            };

            SortedExonsReferenceRegex    = new Regex("\\$VAR1->{'[^']+?'}\\[(\\d+)\\]{'_variation_effect_feature_cache'}{'sorted_exons'}\\[(\\d+)\\][,]?", RegexOptions.Compiled);
            TransExonArrayReferenceRegex = new Regex("\\$VAR1->{'[^']+?'}\\[(\\d+)\\]{'_trans_exon_array'}\\[(\\d+)\\][,]?", RegexOptions.Compiled);
            TranslationReferenceRegex    = new Regex("\\$VAR1->{'[^']+?'}\\[(\\d+)\\]{'translation'}{'([^']+?)'}[,]?", RegexOptions.Compiled);
        }

        /// <summary>
        /// returns a new exon given an ObjectValue
        /// </summary>
        public static DataStructures.VEP.Exon Parse(ObjectValue objectValue, ushort currentReferenceIndex)
        {
            bool onReverseStrand = false;

            int end   = -1;
            byte? phase = null;
            int start = -1;

            string stableId = null;

            // loop over all of the key/value pairs in the exon object
            foreach (AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new GeneralException($"Encountered an unknown key in the dumper mapper object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case Transcript.EndKey:
                        end = DumperUtilities.GetInt32(ad);
                        break;
                    case EndPhaseKey:
                        break;
                    case PhaseKey:
                        int phaseInt = DumperUtilities.GetInt32(ad);
                        if (phaseInt != -1) phase = (byte) phaseInt;
                        break;
                    case Transcript.StableIdKey:
                        stableId = DumperUtilities.GetString(ad);
                        break;
                    case Transcript.StartKey:
                        start = DumperUtilities.GetInt32(ad);
                        break;
                    case Transcript.StrandKey:
                        onReverseStrand = TranscriptUtilities.GetStrand(ad);
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            return new DataStructures.VEP.Exon(currentReferenceIndex, start, end, stableId, onReverseStrand, phase);
        }

        /// <summary>
        /// returns a reference to an exon given an a reference string
        /// </summary>
        public static DataStructures.VEP.Exon ParseReference(string reference, ImportDataStore dataStore)
        {
            var transExonArrayReferenceMatch = TransExonArrayReferenceRegex.Match(reference);
            if (transExonArrayReferenceMatch.Success) return ParseTransExonArrayReference(transExonArrayReferenceMatch, dataStore);

            var sortedExonsReferenceMatch = SortedExonsReferenceRegex.Match(reference);
            if (sortedExonsReferenceMatch.Success) return ParseSortedExonsReference(sortedExonsReferenceMatch, dataStore);

            var translationReferenceMatch = TranslationReferenceRegex.Match(reference);
            if(translationReferenceMatch.Success) return ParseTranslationReference(translationReferenceMatch, dataStore);

            throw new GeneralException($"Unable to use the regular expression on the exon translation reference string: [{reference}]");
        }

        private static DataStructures.VEP.Exon ParseSortedExonsReference(Match referenceMatch, ImportDataStore dataStore)
        {
            int transcriptIndex;
            if (!int.TryParse(referenceMatch.Groups[1].Value, out transcriptIndex))
            {
                throw new GeneralException(
                    $"Unable to convert the transcript index from a string to an integer: [{referenceMatch.Groups[1].Value}]");
            }

            // sanity check: make sure we have at least that many transcripts in our list
            if (transcriptIndex < 0 || transcriptIndex >= dataStore.Transcripts.Count)
            {
                throw new GeneralException(
                    $"Unable to link the exon reference: transcript index: [{transcriptIndex}], current # of transcripts: [{dataStore.Transcripts.Count}]");
            }

            int exonIndex;
            if (!int.TryParse(referenceMatch.Groups[2].Value, out exonIndex))
            {
                throw new GeneralException(
                    $"Unable to convert the exon index from a string to an integer: [{referenceMatch.Groups[2].Value}]");
            }

            // sanity check: make sure we have at least that many exons in our list
            if (exonIndex < 0 || exonIndex >= dataStore.Transcripts[transcriptIndex].VariantEffectCache.Exons.Length)
            {
                throw new GeneralException(
                    $"Unable to link the exon reference: exon index: [{exonIndex}], current # of exons: [{dataStore.Transcripts[transcriptIndex].VariantEffectCache.Exons.Length}]");
            }

            // Console.WriteLine("reference: {0}", reference);
            // Console.WriteLine("transcript index: {0}", transcriptIndex);
            // Console.WriteLine("exon index: {0}", exonIndex);

            return dataStore.Transcripts[transcriptIndex].VariantEffectCache.Exons[exonIndex];
        }

        private static DataStructures.VEP.Exon ParseTransExonArrayReference(Match referenceMatch, ImportDataStore dataStore)
        {
            int transcriptIndex;
            if (!int.TryParse(referenceMatch.Groups[1].Value, out transcriptIndex))
            {
                throw new GeneralException(
                    $"Unable to convert the transcript index from a string to an integer: [{referenceMatch.Groups[1].Value}]");
            }

            // sanity check: make sure we have at least that many transcripts in our list
            if (transcriptIndex < 0 || transcriptIndex >= dataStore.Transcripts.Count)
            {
                throw new GeneralException(
                    $"Unable to link the exon reference: transcript index: [{transcriptIndex}], current # of transcripts: [{dataStore.Transcripts.Count}]");
            }

            int exonIndex;
            if (!int.TryParse(referenceMatch.Groups[2].Value, out exonIndex))
            {
                throw new GeneralException(
                    $"Unable to convert the exon index from a string to an integer: [{referenceMatch.Groups[2].Value}]");
            }

            // sanity check: make sure we have at least that many exons in our list
            if (exonIndex < 0 || exonIndex >= dataStore.Transcripts[transcriptIndex].TransExons.Length)
            {
                throw new GeneralException(
                    $"Unable to link the exon reference: exon index: [{exonIndex}], current # of exons: [{dataStore.Transcripts[transcriptIndex].TransExons.Length}]");
            }

            // Console.WriteLine("reference: {0}", reference);
            // Console.WriteLine("transcript index: {0}", transcriptIndex);
            // Console.WriteLine("exon index: {0}", exonIndex);

            return dataStore.Transcripts[transcriptIndex].TransExons[exonIndex];
        }

        /// <summary>
        /// returns a reference to an exon given a translation reference string
        /// </summary>
        private static DataStructures.VEP.Exon ParseTranslationReference(Match referenceMatch, ImportDataStore dataStore)
        {
            int transcriptIndex;
            if (!int.TryParse(referenceMatch.Groups[1].Value, out transcriptIndex))
            {
                throw new GeneralException(
                    $"Unable to convert the transcript index from a string to an integer: [{referenceMatch.Groups[1].Value}]");
            }

            // sanity check: make sure we have at least that many transcripts in our list
            if (transcriptIndex < 0 || transcriptIndex >= dataStore.Transcripts.Count)
            {
                throw new GeneralException(
                    $"Unable to link the exon reference: transcript index: [{transcriptIndex}], current # of transcripts: [{dataStore.Transcripts.Count}]");
            }

            string exonKey = referenceMatch.Groups[2].Value;

            // Console.WriteLine("reference: {0}", reference);
            // Console.WriteLine("transcript index: {0}", transcriptIndex);
            // Console.WriteLine("exon key: {0}", exonKey);

            DataStructures.VEP.Exon ret;

            switch (exonKey)
            {
                case Translation.EndExonKey:
                    ret = dataStore.Transcripts[transcriptIndex].Translation.EndExon;
                    break;
                case Translation.StartExonKey:
                    ret = dataStore.Transcripts[transcriptIndex].Translation.StartExon;
                    break;
                default:
                    throw new GeneralException($"Unable to determine the correct exon translation to use: {exonKey}");
            }

            return ret;
        }

        /// <summary>
        /// returns an array of exons given a list of ObjectValues (AbstractData)
        /// </summary>
        public static DataStructures.VEP.Exon[] ParseList(List<AbstractData> abstractDataList, ImportDataStore dataStore)
        {
            var exons = new DataStructures.VEP.Exon[abstractDataList.Count];

            // loop over all of the exons
            for (int exonIndex = 0; exonIndex < abstractDataList.Count; exonIndex++)
            {
                // skip references
                if (DumperUtilities.IsReference(abstractDataList[exonIndex])) continue;

                var objectValue = abstractDataList[exonIndex] as ObjectValue;
                if (objectValue != null)
                {
                    var newExon = Parse(objectValue, dataStore.CurrentReferenceIndex);
                    // DS.VEP.Exon oldExon;
                    // if (dataStore.Exons.TryGetValue(newExon, out oldExon))
                    //{
                    //    exons[exonIndex] = oldExon;
                    //}
                    // else
                    //{
                    exons[exonIndex] = newExon;
                    //    dataStore.Exons[newExon] = newExon;
                    //}
                }
                else
                {
                    throw new GeneralException(
                        $"Could not transform the AbstractData object into an ObjectValue: [{abstractDataList[exonIndex].GetType()}]");
                }
            }

            return exons;
        }

        /// <summary>
        /// places a reference to already existing exons into the array of exons
        /// </summary>
        public static void ParseListReference(List<AbstractData> abstractDataList, DataStructures.VEP.Exon[] exons, ImportDataStore dataStore)
        {
            // loop over all of the exons
            for (int exonIndex = 0; exonIndex < abstractDataList.Count; exonIndex++)
            {
                var exonNode = abstractDataList[exonIndex];

                // skip normal exons
                if (!DumperUtilities.IsReference(exonNode)) continue;

                var referenceStringValue = exonNode as ReferenceStringValue;
                if (referenceStringValue != null) exons[exonIndex] = ParseReference(referenceStringValue.Value, dataStore);
            }
        }
    }
}
