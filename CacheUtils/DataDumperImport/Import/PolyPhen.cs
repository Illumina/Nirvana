using System.Collections.Generic;
using System.Text.RegularExpressions;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class PolyPhen
    {
        #region members

        private static readonly HashSet<string> KnownKeys;

        internal const string AnalysisKey           = "analysis";
        internal const string IsMatrixCompressedKey = "matrix_compressed";
        internal const string MatrixKey             = "matrix";
        internal const string PeptideLengthKey      = "peptide_length";
        internal const string SubAnalysisKey        = "sub_analysis";
        // ReSharper disable once InconsistentNaming
        internal const string TranslationMD5Key     = "translation_md5";

        private static readonly Regex PolyPhenReferenceRegex;

        #endregion

        // constructor
        static PolyPhen()
        {
            KnownKeys = new HashSet<string>
            {
                AnalysisKey,
                IsMatrixCompressedKey,
                MatrixKey,
                PeptideLengthKey,
                SubAnalysisKey,
                TranslationMD5Key
            };

            PolyPhenReferenceRegex = new Regex("\\$VAR1->{'[^']+?'}\\[(\\d+)\\]{'_variation_effect_feature_cache'}{'protein_function_predictions'}{'polyphen_humvar'}[,]?", RegexOptions.Compiled);
        }

        /// <summary>
        /// parses the relevant data from each PolyPhen object
        /// </summary>
        public static DataStructures.VEP.PolyPhen Parse(ObjectValue objectValue)
        {
            string matrix = null;

            // loop over all of the key/value pairs in the PolyPhen object
            foreach (AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new GeneralException($"Encountered an unknown key in the dumper PolyPhen object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case AnalysisKey:
                    case IsMatrixCompressedKey:
                    case PeptideLengthKey:
                    case SubAnalysisKey:
                    case TranslationMD5Key:
                        break;
                    case MatrixKey:
                        matrix = DumperUtilities.GetString(ad);
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            return new DataStructures.VEP.PolyPhen(matrix);
        }

        /// <summary>
        /// returns a reference to a PolyPhen object given an a reference string
        /// </summary>
        public static DataStructures.VEP.PolyPhen ParseReference(string reference, ImportDataStore dataStore)
        {
            var polyPhenReferenceMatch = PolyPhenReferenceRegex.Match(reference);

            int transcriptIndex;
            if (!int.TryParse(polyPhenReferenceMatch.Groups[1].Value, out transcriptIndex))
            {
                throw new GeneralException(
                    $"Unable to convert the transcript index from a string to an integer: [{polyPhenReferenceMatch.Groups[1].Value}]");
            }

            // sanity check: make sure we have at least that many transcripts in our list
            if (transcriptIndex < 0 || transcriptIndex >= dataStore.Transcripts.Count)
            {
                throw new GeneralException(
                    $"Unable to link the PolyPhen reference: transcript index: [{transcriptIndex}], current # of transcripts: [{dataStore.Transcripts.Count}]");
            }

            return dataStore.Transcripts[transcriptIndex].VariantEffectCache.ProteinFunctionPredictions.PolyPhen;
        }
    }
}
