using System.Collections.Generic;
using System.Text.RegularExpressions;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.Utilities;
using VariantAnnotation.DataStructures;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures.Intervals;
using VariantAnnotation.DataStructures.Transcript;

namespace CacheUtils.DataDumperImport.Import
{
    public static class Transcript
    {
        #region members

        public const string DataType = "Bio::EnsEMBL::Transcript";

        private const string AttributesKey                  = "attributes";
        private const string BiotypeKey                     = "biotype";
        private const string CcdsKey                        = "_ccds";
        private const string CdnaCodingEndKey               = "cdna_coding_end";
        private const string CdnaCodingStartKey             = "cdna_coding_start";
        private const string CodingRegionEndKey             = "coding_region_end";
        private const string CodingRegionStartKey           = "coding_region_start";
        private const string CreatedDateKey                 = "created_date";
        internal const string DbIdKey                       = "dbID";
        private const string DescriptionKey                 = "description";
        private const string DisplayXrefKey                 = "display_xref";
        internal const string EndKey                        = "end";
        private const string ExternalDbKey                  = "external_db";
        private const string ExternalDisplayNameKey         = "external_display_name";
        private const string ExternalNameKey                = "external_name";
        private const string ExternalStatusKey              = "external_status";
        private const string GeneHgncKey                    = "_gene_hgnc";
        private const string GeneHgncIdKey                  = "_gene_hgnc_id";
        private const string GeneKey                        = "_gene";
        private const string GenePhenotypeKey               = "_gene_phenotype";
        private const string GeneStableIdKey                = "_gene_stable_id";
        private const string GeneSymbolKey                  = "_gene_symbol";
        private const string GeneSymbolSourceKey            = "_gene_symbol_source";
        private const string IsCanonicalKey                 = "is_canonical";
        private const string ModifiedDateKey                = "modified_date";
        private const string ProteinKey                     = "_protein";
        private const string RefseqKey                      = "_refseq";
        internal const string SliceKey                      = "slice";
        private const string SourceKey                      = "source";
        internal const string StableIdKey                   = "stable_id";
        internal const string StartKey                      = "start";
        internal const string StrandKey                     = "strand";
        private const string SwissProtKey                   = "_swissprot";
        private const string TransExonArrayKey              = "_trans_exon_array";
        private const string TranslationKey                 = "translation";
        private const string TremblKey                      = "_trembl";
        private const string UniParcKey                     = "_uniparc";
        private const string VariationEffectFeatureCacheKey = "_variation_effect_feature_cache";
        internal const string VersionKey                    = "version";

        private static readonly HashSet<string> KnownKeys;

        private static readonly Regex TranslationReferenceRegex;

        #endregion

        // constructor
        static Transcript()
        {
            KnownKeys = new HashSet<string>
            {
                AttributesKey,
                BiotypeKey,
                CcdsKey,
                CdnaCodingEndKey,
                CdnaCodingStartKey,
                CodingRegionEndKey,
                CodingRegionStartKey,
                CreatedDateKey,
                DbIdKey,
                DescriptionKey,
                DisplayXrefKey,
                EndKey,
                ExternalDbKey,
                ExternalDisplayNameKey,
                ExternalNameKey,
                ExternalStatusKey,
                GeneHgncKey,
                GeneHgncIdKey,
                GeneKey,
                GenePhenotypeKey,
                GeneStableIdKey,
                GeneSymbolKey,
                GeneSymbolSourceKey,
                IsCanonicalKey,
                ModifiedDateKey,
                ProteinKey,
                RefseqKey,
                SliceKey,
                SourceKey,
                StableIdKey,
                StartKey,
                StrandKey,
                SwissProtKey,
                TransExonArrayKey,
                TranslationKey,
                TremblKey,
                UniParcKey,
                VariationEffectFeatureCacheKey,
                VersionKey
            };

            TranslationReferenceRegex = new Regex("\\$VAR1->{'[^']+?'}\\[(\\d+)\\][,]?", RegexOptions.Compiled);
        }

        /// <summary>
        /// parses the relevant data from each transcript
        /// </summary>
        public static void Parse(ObjectValue objectValue, int transcriptIndex, ImportDataStore dataStore)
        {
            var bioType          = BioType.Unknown;
            var geneSymbolSource = GeneSymbolSource.Unknown; // HGNC

            SimpleInterval[] microRnas                                = null;
			DataStructures.VEP.Exon[] transExons                            = null;
            DataStructures.VEP.Gene gene                                    = null;
            DataStructures.VEP.Translation translation                      = null;
            DataStructures.VEP.VariantEffectFeatureCache variantEffectCache = null;
            DataStructures.VEP.Slice slice                                  = null;

            bool onReverseStrand = false;
            bool isCanonical     = false;

            int compDnaCodingStart = -1;
            int compDnaCodingEnd   = -1;

            int start    = -1;
            int end      = -1;
            byte version = 1;

            string ccdsId       = null;
            string databaseId   = null;
            string proteinId    = null;
            string refSeqId     = null;
            string geneStableId = null;
            string stableId     = null;

            string geneSymbol = null; // DDX11L1
            int hgncId        = -1; // 37102

            // loop over all of the key/value pairs in the transcript object
            foreach (AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new GeneralException($"Encountered an unknown key in the dumper transcript object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case CodingRegionStartKey:
                    case CodingRegionEndKey:
                    case CreatedDateKey:
                    case DescriptionKey:
                    case DisplayXrefKey:
                    case ExternalDbKey:
                    case ExternalDisplayNameKey:
                    case ExternalNameKey:
                    case ExternalStatusKey:
                    case GenePhenotypeKey:
                    case ModifiedDateKey:
                    case SourceKey:
                    case SwissProtKey:
                    case TremblKey:
                    case UniParcKey:
                        // not used
                        break;
                    case AttributesKey:
                        var attributesList = ad as ListObjectKeyValue;
                        if (attributesList != null) microRnas = Attribute.ParseList(attributesList.Values);
                        break;
                    case BiotypeKey:
                        bioType = TranscriptUtilities.GetBiotype(ad);
                        break;
                    case CcdsKey:
                        ccdsId = DumperUtilities.GetString(ad);
                        if (ccdsId == "-" || ccdsId == "") ccdsId = null;
                        break;
                    case CdnaCodingEndKey:
                        compDnaCodingEnd = DumperUtilities.GetInt32(ad);
                        break;
                    case CdnaCodingStartKey:
                        compDnaCodingStart = DumperUtilities.GetInt32(ad);
                        break;
                    case DbIdKey:
                        databaseId = DumperUtilities.GetString(ad);
                        if (databaseId == "-" || databaseId == "") databaseId = null;
                        break;
                    case EndKey:
                        end = DumperUtilities.GetInt32(ad);
                        break;
                    case GeneHgncIdKey:
                        var hgnc = DumperUtilities.GetString(ad);
                        if (hgnc != null && hgnc.StartsWith("HGNC:")) hgnc = hgnc.Substring(5);
                        if (hgnc == "-" || hgnc == "") hgnc = null;
                        
                        if (hgnc != null) hgncId = int.Parse(hgnc);
                        break;
                    case GeneSymbolKey:
                    case GeneHgncKey: // older key
                        geneSymbol = DumperUtilities.GetString(ad);
                        if (geneSymbol == "-" || geneSymbol == "") geneSymbol = null;
                        break;
                    case GeneSymbolSourceKey:
                        geneSymbolSource = TranscriptUtilities.GetGeneSymbolSource(ad);
                        break;
                    case GeneKey:
                        var geneNode = ad as ObjectKeyValue;
                        if (geneNode != null)
                        {
                            gene = Gene.Parse(geneNode.Value, dataStore.CurrentReferenceIndex);
                        }
                        break;
                    case GeneStableIdKey:
                        geneStableId = DumperUtilities.GetString(ad);
                        if (geneStableId == "-" || geneStableId == "") geneStableId = null;
                        break;
                    case IsCanonicalKey:
                        isCanonical = DumperUtilities.GetBool(ad);
                        break;
                    case ProteinKey:
                        proteinId = DumperUtilities.GetString(ad);
                        if (proteinId == "-" || proteinId == "") proteinId = null;
                        break;
                    case RefseqKey:
                        refSeqId = DumperUtilities.GetString(ad);
                        if (refSeqId == "-" || refSeqId == "") refSeqId = null;
                        break;
                    case SliceKey:
                        var sliceNode = ad as ObjectKeyValue;
                        if (sliceNode != null)
                        {
                            slice = Slice.Parse(sliceNode.Value, dataStore.CurrentReferenceIndex);
                        }
                        break;
                    case StableIdKey:
                        stableId = DumperUtilities.GetString(ad);
                        if (stableId == "-" || stableId == "") stableId = null;
                        break;
                    case StartKey:
                        start = DumperUtilities.GetInt32(ad);
                        break;
                    case StrandKey:
                        onReverseStrand = TranscriptUtilities.GetStrand(ad);
                        break;
                    case TransExonArrayKey:
                        var exonsList = ad as ListObjectKeyValue;
                        if (exonsList != null)
                        {
                            transExons = Exon.ParseList(exonsList.Values, dataStore);
                        }
                        else
                        {
                            throw new GeneralException($"Could not transform the AbstractData object into a ListObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    case TranslationKey:
                        var translationNode = ad as ObjectKeyValue;
                        if (translationNode != null)
                        {
                            translation = Translation.Parse(translationNode.Value, dataStore);
                        }
                        else if (DumperUtilities.IsUndefined(ad))
                        {
                            translation = null;
                        }
                        else
                        {
                            throw new GeneralException($"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    case VariationEffectFeatureCacheKey:
                        var cacheNode = ad as ObjectKeyValue;
                        if (cacheNode == null)
                        {
                            throw new GeneralException($"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }
                        variantEffectCache = VariantEffectFeatureCache.Parse(cacheNode.Value, dataStore);
                        break;
                    case VersionKey:
                        version = (byte)DumperUtilities.GetInt32(ad);
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            dataStore.Transcripts.Add(new DataStructures.VEP.Transcript(bioType, transExons, gene, translation, variantEffectCache, slice, 
                onReverseStrand, isCanonical, compDnaCodingStart, compDnaCodingEnd, dataStore.CurrentReferenceIndex, start, end, 
                ccdsId, databaseId, proteinId, refSeqId, geneStableId, stableId, geneSymbol, geneSymbolSource, hgncId, version, 
                microRnas));
        }

        /// <summary>
        /// points to a transcript that has already been created
        /// </summary>
        public static DataStructures.VEP.Transcript ParseReference(string reference, ImportDataStore dataStore)
        {
            var transcriptMatch = TranslationReferenceRegex.Match(reference);

            if (!transcriptMatch.Success)
            {
                throw new GeneralException($"Unable to use the regular expression on the transcript reference string: [{reference}]");
            }

            int transcriptIndex;
            if (!int.TryParse(transcriptMatch.Groups[1].Value, out transcriptIndex))
            {
                throw new GeneralException($"Unable to convert the transcript index from a string to an integer: [{transcriptMatch.Groups[1].Value}]");
            }

            // sanity check: make sure we have at least that many transcripts in our list
            if (transcriptIndex < 0 || transcriptIndex >= dataStore.Transcripts.Count)
            {
                throw new GeneralException($"Unable to link the slice reference: transcript index: [{transcriptIndex}], current # of transcripts: [{dataStore.Transcripts.Count}]");
            }

            return dataStore.Transcripts[transcriptIndex];
        }

        /// <summary>
        /// parses the relevant data from each transcript
        /// </summary>
        public static void ParseReferences(ObjectValue objectValue, int transcriptIndex, ImportDataStore dataStore)
        {
            var transcript = dataStore.Transcripts[transcriptIndex];

            // loop over all of the key/value pairs in the transcript object
            foreach (AbstractData ad in objectValue)
            {
                // skip undefined keys
                if (DumperUtilities.IsUndefined(ad)) continue;

                // handle each key
                ReferenceKeyValue referenceKeyValue;

                // references found in:
                // 'transcript' -> '_variation_effect_feature_cache' -> 'introns' -> 'slice' has references
                // 'transcript' -> 'gene' has references
                // 'transcript' -> 'slice' has references
                // 'transcript' -> '_trans_exon_array' -> [] has references
                // 'transcript' -> 'translation'-> 'end_exon' has references
                // 'transcript' -> 'translation'-> 'start_exon' has references
                // 'transcript' -> 'translation'-> 'transcript' has references

                switch (ad.Key)
                {
                    case GeneKey:
                        // works well
                        if (DumperUtilities.IsReference(ad))
                        {
                            referenceKeyValue = ad as ReferenceKeyValue;
                            if (referenceKeyValue != null)
                                transcript.Gene = Gene.ParseReference(referenceKeyValue.Value, dataStore);
                        }
                        break;
                    case SliceKey:
                        if (DumperUtilities.IsReference(ad))
                        {
                            referenceKeyValue = ad as ReferenceKeyValue;
                            if (referenceKeyValue != null) transcript.Slice = Slice.ParseReference(referenceKeyValue.Value, dataStore);
                        }
                        break;
                    case TransExonArrayKey:
                        var exonsList = ad as ListObjectKeyValue;
                        if (exonsList != null) Exon.ParseListReference(exonsList.Values, transcript.TransExons, dataStore);
                        break;
                    case TranslationKey:
                        var translationNode = ad as ObjectKeyValue;
                        if (translationNode != null) Translation.ParseReference(translationNode.Value, transcript.Translation, dataStore);
                        break;
                    case VariationEffectFeatureCacheKey:
                        var cacheNode = ad as ObjectKeyValue;
                        if (cacheNode != null) VariantEffectFeatureCache.ParseReference(cacheNode.Value, transcript.VariantEffectCache, dataStore);
                        break;
                }
            }
        }
    }
}
