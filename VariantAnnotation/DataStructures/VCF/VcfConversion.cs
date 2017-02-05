using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.VCF
{
    public class VcfConversion
    {
        private readonly StringBuilder _sb             = new StringBuilder();
        private readonly StringBuilder _csqInfoBuilder = new StringBuilder();
        private readonly List<string> _csqtStrings     = new List<string>();
        private readonly List<string> _csqrStrings     = new List<string>();

        public string Convert(IVariant vcfVariant, IAnnotatedVariant annotatedVariant)
        {
            _sb.Clear();

            var fields = vcfVariant.Fields;

            // add all of the fields before the info field
            for (var vcfIndex = 0; vcfIndex < VcfCommon.IdIndex; vcfIndex++)
            {
                _sb.Append(fields[vcfIndex]);
                _sb.Append('\t');
            }

            // add dbSNP id
            var dbSnpId = ExtractDbId(annotatedVariant, fields[VcfCommon.IdIndex]);
            _sb.Append(dbSnpId);
            _sb.Append('\t');

            for (var vcfIndex = VcfCommon.IdIndex + 1; vcfIndex < VcfCommon.InfoIndex; vcfIndex++)
            {
                _sb.Append(fields[vcfIndex]);
                _sb.Append('\t');
            }

            var infoField = (annotatedVariant as UnifiedJson)?.InfoFromVcf;

            AddInfoField(infoField, annotatedVariant, _sb);

            // add all of the fields after the info field
            var numColumns = fields.Length;
            for (var vcfIndex = VcfCommon.InfoIndex + 1; vcfIndex < numColumns; vcfIndex++)
            {
                _sb.Append('\t');
                _sb.Append(fields[vcfIndex]);
            }

            return _sb.ToString();
        }

        private void AddInfoField(string infoField, IAnnotatedVariant annotatedVariant, StringBuilder sb)
        {
            var infoEntries = new VcfField();

            if (!string.IsNullOrEmpty(infoField))
            {
                infoEntries.Add(infoField);
            }

            ExtractInfo(annotatedVariant as UnifiedJson, infoEntries);

            infoField = infoEntries.GetString("");

            // remove .
            if (infoField == ".") infoField = "";

            sb.Append(infoField);

            var csqs = new List<CsqEntry>();

            ExtractCsqs(annotatedVariant as UnifiedJson, csqs);

            if (csqs.Count != 0)
                if (infoField.Length > 0) sb.Append(";");

            // append CSQ tags using delegate from annotator
            sb.Append(GetCsqtAndCsqrVcfInfo(csqs));

            if (csqs.Count == 0 && infoField.Length == 0)
            {
                sb.Append(".");
            }
        }

        /// <summary>
        /// returns the CSQT string as specified by this annotator
        /// </summary>
        private static string GetCsqtString(CsqEntry csq)
        {
            return csq.Allele + '|' +
                   csq.Symbol + '|' +
                   csq.Feature + '|' +
                   csq.Consequence;
        }

        /// <summary>
        /// returns the CSQR string as specified by this annotator
        /// </summary>
        private static string GetCsqrString(CsqEntry csq)
        {
            return csq.Allele + '|' +
                   csq.Feature + '|' +
                   csq.Consequence;
        }

        /// <summary>
        /// returns a concatenated vcf INFO field string containing the CSQT and CSQR tags
        /// </summary>
        private string GetCsqtAndCsqrVcfInfo(List<CsqEntry> csqList)
        {
            // make sure we have some tags
            var numCsqTags = csqList.Count;
            if (numCsqTags == 0) return null;

            // build our vcf INFO fields
            _csqInfoBuilder.Clear();
            _csqtStrings.Clear();
            _csqrStrings.Clear();

            foreach (var csqEntry in csqList)
            {
                // may be null in unit tests
                if (csqEntry.FeatureType == null)
                {
                    // assuming such cases to be transcript type
                    _csqtStrings.Add(GetCsqtString(csqEntry));
                    continue;
                }

                if (csqEntry.FeatureType == CsqCommon.TranscriptFeatureType)
                {
                    _csqtStrings.Add(GetCsqtString(csqEntry));
                }
                else if (csqEntry.FeatureType == CsqCommon.RegulatoryFeatureType)
                {
                    _csqrStrings.Add(GetCsqrString(csqEntry));
                }
            }

            var hasCsqT = _csqtStrings.Count > 0;
            var hasCsqR = _csqrStrings.Count > 0;

            if (hasCsqT) _csqInfoBuilder.Append("CSQT=" + string.Join(",", _csqtStrings));
            if (hasCsqT && hasCsqR) _csqInfoBuilder.Append(';');
            if (hasCsqR) _csqInfoBuilder.Append("CSQR=" + string.Join(",", _csqrStrings));

            return _csqInfoBuilder.ToString();
        }

        private static void ExtractCsqs(UnifiedJson unifiedJson, List<CsqEntry> csqs)
        {
            foreach (var jsonVariant in unifiedJson.AnnotatedAlternateAlleles)
            {
                csqs.AddRange(
                    jsonVariant.EnsemblTranscripts.Where(transcript => transcript.IsCanonical == "true")
                        .Select(transcript => new CsqEntry
                        {
                            Allele = jsonVariant.GenotypeIndex.ToString(),
                            Canonical = transcript.IsCanonical,
                            Feature = transcript.TranscriptID,
                            FeatureType = CsqCommon.TranscriptFeatureType,
                            Symbol = transcript.Hgnc,
                            Consequence = transcript.Consequence == null ? null : string.Join("&", transcript.Consequence)
                        }));

                csqs.AddRange(from transcript in jsonVariant.RefSeqTranscripts
                              where transcript.IsCanonical == "true"
                              select new CsqEntry
                              {
                                  Allele = jsonVariant.GenotypeIndex.ToString(),
                                  Canonical = transcript.IsCanonical,
                                  Feature = transcript.TranscriptID,
                                  FeatureType = CsqCommon.TranscriptFeatureType,
                                  Symbol = transcript.Hgnc,
                                  Consequence = transcript.Consequence == null ? null : string.Join("&", transcript.Consequence)
                              });

                csqs.AddRange(jsonVariant.RegulatoryRegions.Select(regulatoryRegion => new CsqEntry
                {
                    Allele = jsonVariant.GenotypeIndex.ToString(),
                    Consequence = string.Join("&", regulatoryRegion.Consequence),
                    Feature = regulatoryRegion.ID,
                    FeatureType = CsqCommon.RegulatoryFeatureType
                }));
            }
        }

        private static string ExtractDbId(IAnnotatedVariant annotatedVariant, string idField)
        {
            var dbSnp = new VcfField();

            var nonDbsnpIds = GetNonDbsnpIds(idField);
            if (nonDbsnpIds != null) foreach (var nonDbsnpId in nonDbsnpIds) dbSnp.Add(nonDbsnpId);

            foreach (var altAllele in annotatedVariant.AnnotatedAlternateAlleles)
            {
                if (altAllele.DbSnpIds == null) continue;
                foreach (var id in altAllele.DbSnpIds) dbSnp.Add(id);
            }

            return dbSnp.GetString("");
        }

        private static IEnumerable<string> GetNonDbsnpIds(string idField)
        {
            if (idField == null || idField == ".") return null;
            var idList = idField.Split(';').Where(id => !id.StartsWith("rs")).ToList();

            return idList.Count == 0 ? null : idList;
        }

        private static void ExtractInfo(UnifiedJson unifiedJson, VcfField infoEntries)
        {
            var alleleFreq1000G = new VcfInfoKeyValue("AF1000G");
            var ancestralAllele = new VcfInfoKeyValue("AA");
            var clinVar = new VcfInfoKeyValue("clinvar");
            var cosmic = new VcfInfoKeyValue("cosmic");
            var evs = new VcfInfoKeyValue("EVS");
            var globalMinorAllele = new VcfInfoKeyValue("GMAF");
            var phyloP = new VcfInfoKeyValue("phyloP");

            foreach (var jsonVariant in unifiedJson.AnnotatedAlternateAlleles)
            {
                if (jsonVariant.IsReferenceMinor) infoEntries.Add("RefMinor");

                phyloP.Add(jsonVariant.PhylopScore);

                ancestralAllele.Add(jsonVariant.AncestralAllele);

                foreach (var cosmicEntry in jsonVariant.CosmicEntries)
                {
                    if (cosmicEntry.ID == null) continue;
                    if (cosmicEntry.SaAltAllele != jsonVariant.SaAltAllele) continue;

                    cosmic.Add(jsonVariant.GenotypeIndex.ToString(CultureInfo.InvariantCulture) + '|' + cosmicEntry.ID);
                }

                foreach (var clinVarEntry in jsonVariant.ClinVarEntries)
                {
                    if (clinVarEntry.Significance == null) continue;
                    if (clinVarEntry.SaAltAllele != jsonVariant.SaAltAllele) continue;

                    clinVar.Add(jsonVariant.GenotypeIndex.ToString(CultureInfo.InvariantCulture) + '|' + RemoveWhiteSpaceAndComma(clinVarEntry.Significance));
                }

                if (jsonVariant.GlobalMinorAllele != null || jsonVariant.GlobalMinorAlleleFrequency != null)
                {
                    globalMinorAllele.Add(jsonVariant.GlobalMinorAllele + '|' + jsonVariant.GlobalMinorAlleleFrequency);
                }
                else
                {
                    // for multi allelic variants, we need to add a . for the entries that do not have a Global minor allele.
                    globalMinorAllele.Add(null);
                }

                alleleFreq1000G.Add(jsonVariant.AlleleFrequencyAll);
                if (jsonVariant.EvsAlleleFrequencyAll != null || jsonVariant.EvsCoverage != null || jsonVariant.EvsSamples != null)
                {
                    evs.Add(jsonVariant.EvsAlleleFrequencyAll + '|' + jsonVariant.EvsCoverage + '|' + jsonVariant.EvsSamples);
                }
                else
                {
                    evs.Add(null);
                }
            }

            infoEntries.Add(ancestralAllele.GetString());
            infoEntries.Add(globalMinorAllele.GetString());
            infoEntries.Add(alleleFreq1000G.GetString());
            infoEntries.Add(evs.GetString());
            infoEntries.Add(phyloP.GetString());
            infoEntries.Add(cosmic.GetString());
            infoEntries.Add(clinVar.GetString());
        }

        private static string RemoveWhiteSpaceAndComma(string s)
        {
            return s.Replace(' ', '_').Replace(",", "\\x2c");
        }
    }
}
