using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.DataStructures.Import;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CacheUtils.Helpers;
using CacheUtils.Utilities;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Sequence;
using TranscriptUtilities = CacheUtils.DataDumperImport.Utilities.TranscriptUtilities;

namespace CacheUtils.DataDumperImport.Import
{
    public static class ImportTranscript
    {
        private static readonly HashSet<string> KnownKeys;

        static ImportTranscript()
        {
            KnownKeys = new HashSet<string>
            {
                ImportKeys.Attributes,
                ImportKeys.BamEditStatus,
                ImportKeys.Biotype,
                ImportKeys.Ccds,
                ImportKeys.CdnaCodingEnd,
                ImportKeys.CdnaCodingStart,
                ImportKeys.CodingRegionEnd,
                ImportKeys.CodingRegionStart,
                ImportKeys.CreatedDate,
                ImportKeys.DbId,
                ImportKeys.Description,
                ImportKeys.DisplayXref,
                ImportKeys.End,
                ImportKeys.ExternalDb,
                ImportKeys.ExternalDisplayName,
                ImportKeys.ExternalName,
                ImportKeys.ExternalStatus,
                ImportKeys.Gene,
                ImportKeys.GeneHgnc,
                ImportKeys.GeneHgncId,
                ImportKeys.GenePhenotype,
                ImportKeys.GeneStableId,
                ImportKeys.GeneSymbol,
                ImportKeys.GeneSymbolSource,
                ImportKeys.IsCanonical,
                ImportKeys.ModifiedDate,
                ImportKeys.Protein,
                ImportKeys.Refseq,
                ImportKeys.Slice,
                ImportKeys.Source,
                ImportKeys.StableId,
                ImportKeys.Start,
                ImportKeys.Strand,
                ImportKeys.SwissProt,
                ImportKeys.TransExonArray,
                ImportKeys.Translation,
                ImportKeys.Trembl,
                ImportKeys.UniParc,
                ImportKeys.VariationEffectFeatureCache,
                ImportKeys.VepLazyLoaded,
                ImportKeys.Version
            };
        }

        /// <summary>
        /// parses the relevant data from each transcript
        /// </summary>
        public static MutableTranscript Parse(ObjectValueNode objectValue, IChromosome chromosome, Source source)
        {
            // IDs
            string transcriptId    = null;
            byte transcriptVersion = 1;
            string proteinId       = null;
            byte proteinVersion    = 0;
            string ccdsId          = null;
            string refSeqId        = null;
            string geneId          = null;
            int hgncId             = -1;

            // gene
            int geneStart            = -1;
            int geneEnd              = -1;
            bool geneOnReverseStrand = false;
            string geneSymbol        = null;
            var geneSymbolSource     = GeneSymbolSource.Unknown;

            // translation
            int translationStart             = -1;
            int translationEnd               = -1;
            MutableExon translationStartExon = null;
            MutableExon translationEndExon   = null;

            // predictions
            string siftData     = null;
            string polyphenData = null;

            var bioType                        = BioType.other;
            IInterval[] microRnas              = null;
            MutableTranscriptRegion[] cdnaMaps = null;
            IInterval[] introns                = null;
            string peptideSequence             = null;
            string translateableSequence       = null;
            bool isCanonical                   = false;
            int compDnaCodingStart             = -1;
            int compDnaCodingEnd               = -1;
            int start                          = -1;
            int end                            = -1;
            MutableExon[] exons                = null;
            bool cdsStartNotFound              = false;
            bool cdsEndNotFound                = false;
            int[] selenocysteinePositions      = null;
            IRnaEdit[] rnaEdits                = null;
            string bamEditStatus          = null;

            foreach (var node in objectValue.Values)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(node.Key))
                {
                    throw new InvalidDataException($"Encountered an unknown key in the dumper transcript object: {node.Key}");
                }

                // handle each key
                switch (node.Key)
                {
                    case ImportKeys.CodingRegionEnd:
                    case ImportKeys.CodingRegionStart:
                    case ImportKeys.CreatedDate:
                    case ImportKeys.DbId:
                    case ImportKeys.Description:
                    case ImportKeys.DisplayXref:
                    case ImportKeys.ExternalDb:
                    case ImportKeys.ExternalDisplayName:
                    case ImportKeys.ExternalName:
                    case ImportKeys.ExternalStatus:
                    case ImportKeys.GenePhenotype:
                    case ImportKeys.GeneStableId:                    
                    case ImportKeys.ModifiedDate:
                    case ImportKeys.Protein:
                    case ImportKeys.Slice:
                    case ImportKeys.Source:
                    case ImportKeys.Strand:
                    case ImportKeys.SwissProt:
                    case ImportKeys.Trembl:
                    case ImportKeys.UniParc:
                    case ImportKeys.VepLazyLoaded:
                        // not used
                        break;
                    case ImportKeys.BamEditStatus:
                        bamEditStatus = node.GetString();
                        break;
                    case ImportKeys.Attributes:
                        if (node is ListObjectKeyValueNode attributesList) (microRnas, rnaEdits, cdsStartNotFound, cdsEndNotFound) = Attribute.ParseList(attributesList.Values);
                        break;
                    case ImportKeys.Biotype:
                        bioType = TranscriptUtilities.GetBiotype(node);
                        break;
                    case ImportKeys.Ccds:
                        ccdsId = node.GetString();
                        break;
                    case ImportKeys.CdnaCodingEnd:
                        compDnaCodingEnd = node.GetInt32();
                        break;
                    case ImportKeys.CdnaCodingStart:
                        compDnaCodingStart = node.GetInt32();
                        break;
                    case ImportKeys.End:
                        end = node.GetInt32();
                        break;
                    case ImportKeys.GeneHgncId:
                        var hgnc = node.GetString();
                        if (hgnc != null && hgnc.StartsWith("HGNC:")) hgnc = hgnc.Substring(5);
                        if (hgnc != null) hgncId = int.Parse(hgnc);
                        break;
                    case ImportKeys.GeneSymbol:
                    case ImportKeys.GeneHgnc: // older key
                        geneSymbol = node.GetString();
                        break;
                    case ImportKeys.GeneSymbolSource:
                        geneSymbolSource = GeneSymbolSourceHelper.GetGeneSymbolSource(node.GetString());
                        break;
                    case ImportKeys.Gene:
                        if (node is ObjectKeyValueNode geneNode)
                        {
                            (geneStart, geneEnd, geneId, geneOnReverseStrand) = ImportGene.Parse(geneNode.Value);
                        }
                        break;
                    case ImportKeys.IsCanonical:
                        isCanonical = node.GetBool();
                        break;
                    case ImportKeys.Refseq:
                        refSeqId = node.GetString();
                        break;
                    case ImportKeys.StableId:
                        transcriptId = node.GetString();
                        break;
                    case ImportKeys.Start:
                        start = node.GetInt32();
                        break;
                    case ImportKeys.TransExonArray:
                        if (node is ListObjectKeyValueNode exonsList)
                        {
                            exons = ImportExon.ParseList(exonsList.Values, chromosome);
                        }
                        break;
                    case ImportKeys.Translation:
                        if (node is ObjectKeyValueNode translationNode)
                        {
                            (translationStart, translationEnd, proteinId, proteinVersion, translationStartExon, translationEndExon) = ImportTranslation.Parse(translationNode.Value, chromosome);
                        }
                        break;
                    case ImportKeys.VariationEffectFeatureCache:
                        if (node is ObjectKeyValueNode cacheNode)
                        {
                            (cdnaMaps, introns, peptideSequence, translateableSequence, siftData, polyphenData, selenocysteinePositions) = ImportVariantEffectFeatureCache.Parse(cacheNode.Value);
                        }                        
                        break;
                    case ImportKeys.Version:
                        transcriptVersion = (byte)node.GetInt32();
                        break;
                    default:
                        throw new InvalidDataException($"Unknown key found: {node.Key}");
                }
            }

            var fixedTranscript = AccessionUtilities.GetMaxVersion(transcriptId, transcriptVersion);
            var fixedProtein    = AccessionUtilities.GetMaxVersion(proteinId, proteinVersion);

            var gene = new MutableGene(chromosome, geneStart, geneEnd, geneOnReverseStrand, geneSymbol,
                geneSymbolSource, geneId, hgncId);

            var codingRegion = new TranscriptRegion(TranscriptRegionType.CodingRegion, 0,
                GetCodingRegionStart(geneOnReverseStrand, translationStartExon, translationEndExon, translationStart, translationEnd),
                GetCodingRegionEnd(geneOnReverseStrand, translationStartExon, translationEndExon, translationStart, translationEnd), 
                compDnaCodingStart, compDnaCodingEnd);

            var totalExonLength = GetTotalExonLength(exons);
            var startExonPhase  = translationStartExon?.Phase ?? int.MinValue;

            return new MutableTranscript(chromosome, start, end, fixedTranscript.Id, fixedTranscript.Version, ccdsId,
                refSeqId, bioType, isCanonical, codingRegion, fixedProtein.Id, fixedProtein.Version,
                peptideSequence, source, gene, exons, startExonPhase, totalExonLength, introns, cdnaMaps,
                siftData, polyphenData, translateableSequence, microRnas, cdsStartNotFound, cdsEndNotFound,
                selenocysteinePositions, rnaEdits, bamEditStatus);
        }

        /// <summary>
        /// returns the start position of the coding region. Returns -1 if no translation was possible.
        /// </summary>
        private static int GetCodingRegionStart(bool onReverseStrand, MutableExon startExon, MutableExon endExon,
            int translationStart, int translationEnd)
        {
            if (startExon == null || endExon == null) return -1;
            return onReverseStrand
                ? endExon.End - translationEnd + 1
                : startExon.Start + translationStart - 1;
        }

        /// <summary>
        /// returns the start position of the coding region. Returns -1 if no translation was possible.
        /// </summary>
        private static int GetCodingRegionEnd(bool onReverseStrand, MutableExon startExon, MutableExon endExon,
            int translationStart, int translationEnd)
        {
            if (startExon == null || endExon == null) return -1;
            return onReverseStrand
                ? startExon.End - translationStart + 1
                : endExon.Start + translationEnd - 1;
        }

        /// <summary>
        /// returns the sum of the exon lengths
        /// </summary>
        private static int GetTotalExonLength(IEnumerable<MutableExon> exons) => exons.Sum(exon => exon.End - exon.Start + 1);
    }
}
