using System.Collections.Generic;
using System.Text.RegularExpressions;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class Slice
    {
        #region members

        private const string CoordSystemKey        = "coord_system";
        private const string CircularKey           = "circular";
        private const string SequenceRegionLenKey  = "seq_region_length";
        private const string SequenceRegionNameKey = "seq_region_name";
        private const string TopLevelSliceKey      = "toplevel";

        private static readonly HashSet<string> KnownKeys;

        private static readonly Regex CacheReferenceRegex;
        private static readonly Regex ReferenceRegex;

        #endregion

        // constructor
        static Slice()
        {
            KnownKeys = new HashSet<string>
            {
                CircularKey,
                CoordSystemKey,
                Transcript.EndKey,
                SequenceRegionLenKey,
                SequenceRegionNameKey,
                Transcript.StartKey,
                Transcript.StrandKey,
                TopLevelSliceKey
            };

            CacheReferenceRegex = new Regex("\\$VAR1->{'[^']+?'}\\[(\\d+)\\]{'_variation_effect_feature_cache'}{'introns'}\\[(\\d+)\\]{'slice'}[,]?", RegexOptions.Compiled);
            ReferenceRegex      = new Regex("\\$VAR1->{'[^']+?'}\\[(\\d+)\\]{'slice'}[,]?", RegexOptions.Compiled);
        }

        /// <summary>
        /// parses the relevant data from each slice
        /// </summary>
        public static DataStructures.VEP.Slice Parse(ObjectValue objectValue, ushort currentReferenceIndex)
        {
            DataStructures.VEP.CoordSystem coordinateSystem = null;

            bool isCircular           = false;
            bool isTopLevel           = false;
            bool onReverseStrand      = false;

            int start                 = -1;
            int end                   = -1;

            int sequenceRegionLen     = -1;
            string sequenceRegionName = null;

            // loop over all of the key/value pairs in the gene object
            foreach (AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new GeneralException($"Encountered an unknown key in the dumper slice object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case CoordSystemKey:
                        var coordSystemNode = ad as ObjectKeyValue;
                        if (coordSystemNode != null) coordinateSystem = CoordSystem.Parse(coordSystemNode.Value);
                        break;
                    case CircularKey:
                        isCircular = DumperUtilities.GetBool(ad);
                        break;
                    case Transcript.EndKey:
                        end = DumperUtilities.GetInt32(ad);
                        break;
                    case SequenceRegionLenKey:
                        sequenceRegionLen = DumperUtilities.GetInt32(ad);
                        break;
                    case SequenceRegionNameKey:
                        sequenceRegionName = DumperUtilities.GetString(ad);
                        break;
                    case Transcript.StartKey:
                        start = DumperUtilities.GetInt32(ad);
                        break;
                    case Transcript.StrandKey:
                        onReverseStrand = TranscriptUtilities.GetStrand(ad);
                        break;
                    case TopLevelSliceKey:
                        isTopLevel = DumperUtilities.GetBool(ad);
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            return new DataStructures.VEP.Slice(currentReferenceIndex, start, end, onReverseStrand, isCircular, isTopLevel, coordinateSystem, sequenceRegionLen, sequenceRegionName);
        }

        /// <summary>
        /// points to a slice that has already been created
        /// </summary>
        public static DataStructures.VEP.Slice ParseReference(string reference, ImportDataStore dataStore)
        {
            var sliceReferenceMatch = ReferenceRegex.Match(reference);

            if (!sliceReferenceMatch.Success)
            {
                return ParseCacheReference(reference, dataStore);
            }

            int transcriptIndex;
            if (!int.TryParse(sliceReferenceMatch.Groups[1].Value, out transcriptIndex))
            {
                throw new GeneralException(
                    $"Unable to convert the transcript index from a string to an integer: [{sliceReferenceMatch.Groups[1].Value}]");
            }

            // sanity check: make sure we have at least that many transcripts in our list
            if (transcriptIndex < 0 || transcriptIndex >= dataStore.Transcripts.Count)
            {
                throw new GeneralException(
                    $"Unable to link the slice reference: transcript index: [{transcriptIndex}], current # of transcripts: [{dataStore.Transcripts.Count}]");
            }

            return dataStore.Transcripts[transcriptIndex].Slice;
        }

        /// <summary>
        /// points to a slice that has already been created
        /// </summary>
        private static DataStructures.VEP.Slice ParseCacheReference(string reference, ImportDataStore dataStore)
        {
            var sliceReferenceMatch = CacheReferenceRegex.Match(reference);

            if (!sliceReferenceMatch.Success)
            {
                throw new GeneralException(
                    $"Unable to use the regular expression on the slice reference string: [{reference}]");
            }

            int transcriptIndex;
            if (!int.TryParse(sliceReferenceMatch.Groups[1].Value, out transcriptIndex))
            {
                throw new GeneralException(
                    $"Unable to convert the transcript index from a string to an integer: [{sliceReferenceMatch.Groups[1].Value}]");
            }

            // sanity check: make sure we have at least that many transcripts in our list
            if (transcriptIndex < 0 || transcriptIndex >= dataStore.Transcripts.Count)
            {
                throw new GeneralException(
                    $"Unable to link the slice reference: transcript index: [{transcriptIndex}], current # of transcripts: [{dataStore.Transcripts.Count}]");
            }

            int intronIndex;
            if (!int.TryParse(sliceReferenceMatch.Groups[2].Value, out intronIndex))
            {
                throw new GeneralException(
                    $"Unable to convert the intron index from a string to an integer: [{sliceReferenceMatch.Groups[2].Value}]");
            }

            // sanity check: make sure we have at least that many introns in our list
            if (intronIndex < 0 || intronIndex >= dataStore.Transcripts[transcriptIndex].VariantEffectCache.Introns.Length)
            {
                throw new GeneralException(
                    $"Unable to link the intron reference: intron index: [{intronIndex}], current # of introns: [{dataStore.Transcripts[transcriptIndex].VariantEffectCache.Introns.Length}]");
            }

            // Console.WriteLine("reference: {0}", reference);
            // Console.WriteLine("transcript index: {0}", transcriptIndex);
            // Console.WriteLine("intron index: {0}", intronIndex);
            // Environment.Exit(1);

            return dataStore.Transcripts[transcriptIndex].VariantEffectCache.Introns[intronIndex].Slice;
        }
    }
}
