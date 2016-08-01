using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Illumina.DataDumperImport.Utilities;
using DS = Illumina.DataDumperImport.DataStructures;

namespace Illumina.DataDumperImport.Import
{
    internal static class Gene
    {
        #region members

        private static readonly HashSet<string> KnownKeys;
        private static readonly Regex ReferenceRegex;

        #endregion

        // constructor
        static Gene()
        {
            KnownKeys = new HashSet<string>
            {
                Transcript.EndKey,
                Transcript.StableIdKey,
                Transcript.StartKey,
                Transcript.StrandKey
            };

            ReferenceRegex = new Regex("\\$VAR1->{'[^']+?'}\\[(\\d+)\\]{'_gene'}[,]?", RegexOptions.Compiled);
        }

        /// <summary>
        /// returns a new gene given an ObjectValue
        /// </summary>
        public static DS.VEP.Gene Parse(DS.ObjectValue objectValue, ushort currentReferenceIndex)
        {
            int start            = -1;
            int end              = -1;
            string stableId      = null;
            bool onReverseStrand = false;

            // loop over all of the key/value pairs in the gene object
            foreach (DS.AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new ApplicationException($"Encountered an unknown key in the dumper gene object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case Transcript.EndKey:
                        end = DumperUtilities.GetInt32(ad);
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
                        throw new ApplicationException($"Unknown key found: {ad.Key}");
                }
            }

            return new DS.VEP.Gene(currentReferenceIndex, start, end, stableId, onReverseStrand);
        }

        /// <summary>
        /// returns a reference to a gene given an a reference string
        /// </summary>
        public static DS.VEP.Gene ParseReference(string reference, DS.ImportDataStore dataStore)
        {
            var geneReferenceMatch = ReferenceRegex.Match(reference);

            if (!geneReferenceMatch.Success)
            {
                throw new ApplicationException(
                    $"Unable to use the regular expression on the gene reference string: [{reference}]");
            }

            int transcriptIndex;
            if (!int.TryParse(geneReferenceMatch.Groups[1].Value, out transcriptIndex))
            {
                throw new ApplicationException(
                    $"Unable to convert the transcript index from a string to an integer: [{geneReferenceMatch.Groups[1].Value}]");
            }

            // sanity check: make sure we have at least that many transcripts in our list
            if ((transcriptIndex < 0) || (transcriptIndex >= dataStore.Transcripts.Count))
            {
                throw new ApplicationException(
                    $"Unable to link the gene reference: transcript index: [{transcriptIndex}], current # of transcripts: [{dataStore.Transcripts.Count}]");
            }

            return dataStore.Transcripts[transcriptIndex].Gene;
        }
    }
}
