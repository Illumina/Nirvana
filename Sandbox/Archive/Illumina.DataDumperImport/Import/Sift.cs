using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Illumina.DataDumperImport.Utilities;
using DS = Illumina.DataDumperImport.DataStructures;

namespace Illumina.DataDumperImport.Import
{
    internal static class Sift
    {
        #region members

        private static readonly HashSet<string> KnownKeys;

        private static readonly Regex SiftReferenceRegex;

        #endregion

        // constructor
        static Sift()
        {
            KnownKeys = new HashSet<string>
            {
                PolyPhen.AnalysisKey,
                PolyPhen.IsMatrixCompressedKey,
                PolyPhen.MatrixKey,
                PolyPhen.PeptideLengthKey,
                PolyPhen.SubAnalysisKey,
                PolyPhen.TranslationMD5Key
            };

            SiftReferenceRegex = new Regex("\\$VAR1->{'[^']+?'}\\[(\\d+)\\]{'_variation_effect_feature_cache'}{'protein_function_predictions'}{'sift'}[,]?", RegexOptions.Compiled);
        }

        /// <summary>
        /// parses the relevant data from each sift object
        /// </summary>
        public static DS.VEP.Sift Parse(DS.ObjectValue objectValue)
        {
            string matrix = null;

            // loop over all of the key/value pairs in the sift object
            foreach (DS.AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new ApplicationException($"Encountered an unknown key in the dumper sift object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case PolyPhen.AnalysisKey:
                    case PolyPhen.IsMatrixCompressedKey:
                    case PolyPhen.PeptideLengthKey:
                    case PolyPhen.SubAnalysisKey:
                    case PolyPhen.TranslationMD5Key:
                        break;
                    case PolyPhen.MatrixKey:
                        matrix = DumperUtilities.GetString(ad);
                        break;
                    default:
                        throw new ApplicationException($"Unknown key found: {ad.Key}");
                }
            }

            return new DS.VEP.Sift(matrix);
        }

        /// <summary>
        /// returns a reference to a Sift object given an a reference string
        /// </summary>
        public static DS.VEP.Sift ParseReference(string reference, DS.ImportDataStore dataStore)
        {
            var siftReferenceMatch = SiftReferenceRegex.Match(reference);

            int transcriptIndex;
            if (!int.TryParse(siftReferenceMatch.Groups[1].Value, out transcriptIndex))
            {
                throw new ApplicationException(
                    $"Unable to convert the transcript index from a string to an integer: [{siftReferenceMatch.Groups[1].Value}]");
            }

            // sanity check: make sure we have at least that many transcripts in our list
            if ((transcriptIndex < 0) || (transcriptIndex >= dataStore.Transcripts.Count))
            {
                throw new ApplicationException(
                    $"Unable to link the Sift reference: transcript index: [{transcriptIndex}], current # of transcripts: [{dataStore.Transcripts.Count}]");
            }

            return dataStore.Transcripts[transcriptIndex].VariantEffectCache.ProteinFunctionPredictions.Sift;
        }
    }
}
