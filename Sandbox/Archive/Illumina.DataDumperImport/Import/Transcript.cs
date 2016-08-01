using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Illumina.VariantAnnotation.DataStructures;
using DS = Illumina.DataDumperImport.DataStructures;
using Illumina.DataDumperImport.Utilities;

namespace Illumina.DataDumperImport.Import
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
        internal const string DbIdKey                        = "dbID";
        private const string DescriptionKey                 = "description";
        private const string DisplayXrefKey                 = "display_xref";
        internal const string EndKey                         = "end";
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
        internal const string SliceKey                       = "slice";
        private const string SourceKey                      = "source";
        internal const string StableIdKey                    = "stable_id";
        internal const string StartKey                       = "start";
        internal const string StrandKey                      = "strand";
        private const string SwissProtKey                   = "_swissprot";
        private const string TransExonArrayKey              = "_trans_exon_array";
        private const string TranslationKey                 = "translation";
        private const string TremblKey                      = "_trembl";
        private const string UniParcKey                     = "_uniparc";
        private const string VariationEffectFeatureCacheKey = "_variation_effect_feature_cache";
        internal const string VersionKey                     = "version";

        private static readonly HashSet<string> KnownKeys;

        private static readonly Regex TranslationReferenceRegex;

        private static readonly Dictionary<string, int> AccessionToGeneId   = new Dictionary<string, int>();
        private static readonly Dictionary<int, DS.GeneInfo> GeneIdToSymbol = new Dictionary<int, DS.GeneInfo>();
        private static readonly Dictionary<int, DS.GeneInfo> HgncIdToSymbol = new Dictionary<int, DS.GeneInfo>();

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

        public static string RemoveVersion(string id)
        {
            if (id == null) return null;
            int lastPeriod = id.LastIndexOf('.');
            if (lastPeriod == -1) return id;

            return id.Substring(0, lastPeriod);
        }

        public static void AddGeneIdToSymbol(int geneId, DS.GeneInfo geneInfo)
        {
            if (!GeneIdToSymbol.ContainsKey(geneId))
            {
                GeneIdToSymbol[geneId] = geneInfo;
            }
        }

        public static void AddHgncIdToSymbol(int hgncId, DS.GeneInfo geneInfo)
        {
            if (!HgncIdToSymbol.ContainsKey(hgncId))
            {
                HgncIdToSymbol[hgncId] = geneInfo;
            }
        }

        public static void AddAccessionToGeneId(string transcriptId, int geneId)
        {
            if (!AccessionToGeneId.ContainsKey(transcriptId))
            {
                AccessionToGeneId[transcriptId] = geneId;
            }
        }

        /// <summary>
        /// parses the relevant data from each transcript
        /// </summary>
        public static void Parse(DS.ObjectValue objectValue, int transcriptIndex, DS.ImportDataStore dataStore)
        {
            // Console.WriteLine("*** Parse {0} ***", transcriptIndex + 1);

            var bioType          = BioType.Unknown;
            var geneSymbolSource = GeneSymbolSource.Unknown; // HGNC

            MicroRna[] microRnas                                = null;
			DS.VEP.Exon[] transExons                            = null;
            DS.VEP.Gene gene                                    = null;
            DS.VEP.Translation translation                      = null;
            DS.VEP.VariantEffectFeatureCache variantEffectCache = null;
            DS.VEP.Slice slice                                  = null;

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
            string hgncId     = null; // 37102

            // loop over all of the key/value pairs in the transcript object
            foreach (DS.AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new ApplicationException($"Encountered an unknown key in the dumper transcript object: {ad.Key}");
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
                        var attributesList = ad as DS.ListObjectKeyValue;
                        if (attributesList != null) microRnas = Attribute.ParseList(attributesList.Values);
                        break;
                    case BiotypeKey:
                        bioType = TranscriptUtilities.GetBiotype(ad);
                        break;
                    case CcdsKey:
                        ccdsId = DumperUtilities.GetString(ad);
                        if ((ccdsId == "-") || (ccdsId == "")) ccdsId = null;
                        break;
                    case CdnaCodingEndKey:
                        compDnaCodingEnd = DumperUtilities.GetInt32(ad);
                        break;
                    case CdnaCodingStartKey:
                        compDnaCodingStart = DumperUtilities.GetInt32(ad);
                        break;
                    case DbIdKey:
                        databaseId = DumperUtilities.GetString(ad);
                        if ((databaseId == "-") || (databaseId == "")) databaseId = null;
                        break;
                    case EndKey:
                        end = DumperUtilities.GetInt32(ad);
                        break;
                    case GeneHgncIdKey:
                        hgncId = DumperUtilities.GetString(ad);
                        if ((hgncId == "-") || (hgncId == "")) hgncId = null;
                        break;
                    case GeneSymbolKey:
                    case GeneHgncKey: // older key
                        geneSymbol = DumperUtilities.GetString(ad);
                        if ((geneSymbol == "-") || (geneSymbol == "")) geneSymbol = null;
                        break;
                    case GeneSymbolSourceKey:
                        geneSymbolSource = TranscriptUtilities.GetGeneSymbolSource(ad);
                        break;
                    case GeneKey:
                        var geneNode = ad as DS.ObjectKeyValue;
                        if (geneNode != null)
                        {
                            var newGene = Gene.Parse(geneNode.Value, dataStore.CurrentReferenceIndex);
                            // DS.VEP.Gene oldGene;
                            // if (dataStore.Genes.TryGetValue(newGene, out oldGene))
                            //{
                            //    gene = oldGene;
                            //}
                            // else
                            //{
                            gene = newGene;
                            //    dataStore.Genes[newGene] = newGene;
                            //}
                        }
                        break;
                    case GeneStableIdKey:
                        geneStableId = DumperUtilities.GetString(ad);
                        if ((geneStableId == "-") || (geneStableId == "")) geneStableId = null;
                        break;
                    case IsCanonicalKey:
                        isCanonical = DumperUtilities.GetBool(ad);
                        break;
                    case ProteinKey:
                        proteinId = DumperUtilities.GetString(ad);
                        if ((proteinId == "-") || (proteinId == "")) proteinId = null;
                        break;
                    case RefseqKey:
                        refSeqId = DumperUtilities.GetString(ad);
                        if ((refSeqId == "-") || (refSeqId == "")) refSeqId = null;
                        break;
                    case SliceKey:
                        var sliceNode = ad as DS.ObjectKeyValue;
                        if (sliceNode != null)
                        {
                            var newSlice = Slice.Parse(sliceNode.Value, dataStore.CurrentReferenceIndex);
                            // DS.VEP.Slice oldSlice;
                            // if (dataStore.Slices.TryGetValue(newSlice, out oldSlice))
                            //{
                            //    slice = oldSlice;
                            //}
                            // else
                            //{
                            slice = newSlice;
                            //    dataStore.Slices[newSlice] = newSlice;
                            //}
                        }
                        break;
                    case StableIdKey:
                        stableId = DumperUtilities.GetString(ad);
                        if ((stableId == "-") || (stableId == "")) stableId = null;
                        break;
                    case StartKey:
                        start = DumperUtilities.GetInt32(ad);
                        break;
                    case StrandKey:
                        onReverseStrand = TranscriptUtilities.GetStrand(ad);
                        break;
                    case TransExonArrayKey:
                        var exonsList = ad as DS.ListObjectKeyValue;
                        if (exonsList != null)
                        {
                            transExons = Exon.ParseList(exonsList.Values, dataStore);
                        }
                        else
                        {
                            throw new ApplicationException($"Could not transform the AbstractData object into a ListObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    case TranslationKey:
                        var translationNode = ad as DS.ObjectKeyValue;
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
                            throw new ApplicationException($"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    case VariationEffectFeatureCacheKey:
                        var cacheNode = ad as DS.ObjectKeyValue;
                        if (cacheNode == null)
                        {
                            throw new ApplicationException($"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }
                        variantEffectCache = VariantEffectFeatureCache.Parse(cacheNode.Value, dataStore);
                        break;
                    case VersionKey:
                        version = (byte)DumperUtilities.GetInt32(ad);
                        break;
                    default:
                        throw new ApplicationException($"Unknown key found: {ad.Key}");
                }
            }

            // NOTE: we always seem to have the gene ID, but not necessarily the symbol
            DS.GeneInfo geneInfo;

            // lookup the gene ID if we need to
            if (geneStableId.StartsWith("CCDS"))
            {
                if (string.IsNullOrEmpty(refSeqId))
                {
                    throw new ApplicationException($"Found a null RefSeq ID for the following transcript: {stableId}");
                }

                var refSeqIds = refSeqId.Split(',');
                
                foreach (var refSeq in refSeqIds)
                {
                    int tempGeneId;

                    if (AccessionToGeneId.TryGetValue(refSeq, out tempGeneId))
                    {
                        geneStableId = tempGeneId.ToString();
                        break;
                    }
                }
            }

            if (hgncId == null)
            {
                // lookup the HGNC id, gene symbol, and gene symbol source
                int geneId;
                if (int.TryParse(geneStableId, out geneId))
                {
                    // lookup the gene symbol given the gene ID
                    if (GeneIdToSymbol.TryGetValue(geneId, out geneInfo))
                    {
                        geneSymbol       = geneInfo.GeneSymbol;
                        geneSymbolSource = geneInfo.GeneSymbolSource;
                        if (geneInfo.HgncId != null) hgncId = geneInfo.HgncId.ToString();
                    }
                }
            }
            else
            {
                // update the gene symbol

                // get rid of the HGNC: that may show up
                if (hgncId.StartsWith("HGNC:")) hgncId = hgncId.Substring(5);

                // lookup the gene symbol given the HGNC ID
                int? hgncIdNum = null;
                if (!string.IsNullOrEmpty(hgncId)) hgncIdNum = int.Parse(hgncId);

                if ((hgncIdNum != null) && HgncIdToSymbol.TryGetValue((int)hgncIdNum, out geneInfo))
                {
                    geneSymbol = geneInfo.GeneSymbol;
                }
            }

            if (string.IsNullOrEmpty(geneSymbol))
            {
                if (!stableId.StartsWith("ENSEST"))
                {
                    Console.WriteLine($"Transcript ID: [{stableId}]");
                    Console.WriteLine($"Gene ID:       [{geneStableId}]");
                    Console.WriteLine($"RefSeq ID:     [{refSeqId}]");
                    Console.WriteLine($"HGNC ID:       [{hgncId}]");
                    Console.WriteLine();
                }
            }

            dataStore.Transcripts.Add(new DS.VEP.Transcript(bioType, transExons, gene, translation, variantEffectCache, slice, 
                onReverseStrand, isCanonical, compDnaCodingStart, compDnaCodingEnd, dataStore.CurrentReferenceIndex, start, end, 
                ccdsId, databaseId, proteinId, refSeqId, geneStableId, stableId, geneSymbol, geneSymbolSource, hgncId, version, 
                microRnas));
        }

        /// <summary>
        /// points to a transcript that has already been created
        /// </summary>
        public static DS.VEP.Transcript ParseReference(string reference, DS.ImportDataStore dataStore)
        {
            var transcriptMatch = TranslationReferenceRegex.Match(reference);

            if (!transcriptMatch.Success)
            {
                throw new ApplicationException($"Unable to use the regular expression on the transcript reference string: [{reference}]");
            }

            int transcriptIndex;
            if (!int.TryParse(transcriptMatch.Groups[1].Value, out transcriptIndex))
            {
                throw new ApplicationException($"Unable to convert the transcript index from a string to an integer: [{transcriptMatch.Groups[1].Value}]");
            }

            // sanity check: make sure we have at least that many transcripts in our list
            if ((transcriptIndex < 0) || (transcriptIndex >= dataStore.Transcripts.Count))
            {
                throw new ApplicationException($"Unable to link the slice reference: transcript index: [{transcriptIndex}], current # of transcripts: [{dataStore.Transcripts.Count}]");
            }

            return dataStore.Transcripts[transcriptIndex];
        }

        /// <summary>
        /// parses the relevant data from each transcript
        /// </summary>
        public static void ParseReferences(DS.ObjectValue objectValue, int transcriptIndex, DS.ImportDataStore dataStore)
        {
            // Console.WriteLine("*** ParseReferences {0} / {1} ***", transcriptIndex + 1, _tempTranscripts.Count);
            var transcript = dataStore.Transcripts[transcriptIndex];

            // loop over all of the key/value pairs in the transcript object
            foreach (DS.AbstractData ad in objectValue)
            {
                // skip undefined keys
                if (DumperUtilities.IsUndefined(ad)) continue;

                // handle each key
                DS.ReferenceKeyValue referenceKeyValue;

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
                            referenceKeyValue = ad as DS.ReferenceKeyValue;
                            if (referenceKeyValue != null)
                                transcript.Gene = Gene.ParseReference(referenceKeyValue.Value, dataStore);
                        }
                        break;
                    case SliceKey:
                        if (DumperUtilities.IsReference(ad))
                        {
                            referenceKeyValue = ad as DS.ReferenceKeyValue;
                            if (referenceKeyValue != null) transcript.Slice = Slice.ParseReference(referenceKeyValue.Value, dataStore);
                        }
                        break;
                    case TransExonArrayKey:
                        var exonsList = ad as DS.ListObjectKeyValue;
                        if (exonsList != null) Exon.ParseListReference(exonsList.Values, transcript.TransExons, dataStore);
                        break;
                    case TranslationKey:
                        var translationNode = ad as DS.ObjectKeyValue;
                        if (translationNode != null) Translation.ParseReference(translationNode.Value, transcript.Translation, dataStore);
                        break;
                    case VariationEffectFeatureCacheKey:
                        var cacheNode = ad as DS.ObjectKeyValue;
                        if (cacheNode != null) VariantEffectFeatureCache.ParseReference(cacheNode.Value, transcript.VariantEffectCache, dataStore);
                        break;
                }
            }
        }
    }
}
