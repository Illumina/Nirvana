using System.Collections.Generic;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class VariantEffectFeatureCache
    {
        #region members

        private const string CodonTableKey                 = "codon_table";
        private const string IntronsKey                    = "introns";
        private const string MapperKey                     = "mapper";
        private const string PeptideKey                    = "peptide";
        private const string ProteinFeaturesKey            = "protein_features";
        private const string ProteinFunctionPredictionsKey = "protein_function_predictions";
        private const string SelenocysteinesKey            = "selenocysteines";
        private const string SeqEditsKey                   = "seq_edits";
        private const string SortedExonsKey                = "sorted_exons";
        private const string ThreePrimeUtrKey              = "three_prime_utr";
        private const string TranslateableSeqKey           = "translateable_seq";

        private static readonly HashSet<string> KnownKeys;

        #endregion

        // constructor
        static VariantEffectFeatureCache()
        {
            KnownKeys = new HashSet<string>
            {
                CodonTableKey,
                IntronsKey,
                MapperKey,
                PeptideKey,
                ProteinFeaturesKey,
                ProteinFunctionPredictionsKey,
                SelenocysteinesKey,
                SeqEditsKey,
                SortedExonsKey,
                ThreePrimeUtrKey,
                TranslateableSeqKey
            };
        }

        /// <summary>
        /// parses the relevant data from each variant effect feature cache
        /// </summary>
        public static DataStructures.VEP.VariantEffectFeatureCache Parse(ObjectValue objectValue, ImportDataStore dataStore)
        {
            var cache = new DataStructures.VEP.VariantEffectFeatureCache();

            // loop over all of the key/value pairs in the cache object
            foreach (AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new GeneralException(
                        $"Encountered an unknown key in the dumper variant effect feature cache object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case SelenocysteinesKey:
                    case ThreePrimeUtrKey:
                    case SeqEditsKey:
                    case CodonTableKey:
                    case ProteinFeaturesKey:
                        // not used
                        break;
                    case IntronsKey:
                        var intronsList = ad as ListObjectKeyValue;
                        if (intronsList != null)
                        {
                            cache.Introns = Intron.ParseList(intronsList.Values, dataStore);
                        }
                        else if (DumperUtilities.IsUndefined(ad))
                        {
                            cache.Introns = null;
                        }
                        else
                        {
                            throw new GeneralException(
                                $"Could not transform the AbstractData object into a ListObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    case MapperKey:
                        var mapperNode = ad as ObjectKeyValue;
                        if (mapperNode != null)
                        {
                            cache.Mapper = TranscriptMapper.Parse(mapperNode.Value, dataStore);
                        }
                        else
                        {
                            throw new GeneralException(
                                $"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    case PeptideKey:
                        cache.Peptide = DumperUtilities.GetString(ad);
                        break;
                    case ProteinFunctionPredictionsKey:
                        var predictionsNode = ad as ObjectKeyValue;
                        if (predictionsNode != null)
                        {
                            cache.ProteinFunctionPredictions = ProteinFunctionPredictions.Parse(predictionsNode.Value);
                        }
                        else
                        {
                            throw new GeneralException(
                                $"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }

                        break;
                    case SortedExonsKey:
                        var exonsList = ad as ListObjectKeyValue;
                        if (exonsList != null)
                        {
                            cache.Exons = Exon.ParseList(exonsList.Values, dataStore);
                        }
                        else
                        {
                            throw new GeneralException($"Could not transform the AbstractData object into a ListObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    case TranslateableSeqKey:
                        cache.TranslateableSeq = DumperUtilities.GetString(ad);
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            return cache;
        }

        /// <summary>
        /// parses the relevant data from each variant effect feature cache
        /// </summary>
        public static void ParseReference(ObjectValue objectValue, DataStructures.VEP.VariantEffectFeatureCache cache, ImportDataStore dataStore)
        {
            // loop over all of the key/value pairs in the cache object
            foreach (AbstractData ad in objectValue)
            {
                switch (ad.Key)
                {
                    case IntronsKey:
                        var intronsList = ad as ListObjectKeyValue;
                        if (intronsList != null) Intron.ParseListReference(intronsList.Values, cache.Introns, dataStore);
                        break;
                    case MapperKey:
                        var mapperNode = ad as ObjectKeyValue;
                        if (mapperNode != null)
                        {
                            TranscriptMapper.ParseReference(mapperNode.Value, cache.Mapper, dataStore);
                        }
                        break;
                    case ProteinFunctionPredictionsKey:
                        var predictionsNode = ad as ObjectKeyValue;
                        if (predictionsNode != null)
                        {
                            ProteinFunctionPredictions.ParseReference(predictionsNode.Value, cache.ProteinFunctionPredictions, dataStore);
                        }
                        break;
                }
            }
        }
    }
}
