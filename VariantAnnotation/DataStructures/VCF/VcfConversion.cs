using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.FileHandling.VCF;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.VCF
{
    public class VcfConversion
    {
		private const string DbSnpKeyName = "dbsnp";
		private const string OneKgKeyName = "oneKg";
		private const string RefMinorKeyName = "RefMinor";

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
                            Allele      = jsonVariant.GenotypeIndex.ToString(),
                            Feature     = transcript.TranscriptID,
                            FeatureType = CsqCommon.TranscriptFeatureType,
                            Symbol      = transcript.Hgnc,
                            Consequence = transcript.Consequence == null ? null : string.Join("&", transcript.Consequence)
                        }));

                csqs.AddRange(from transcript in jsonVariant.RefSeqTranscripts
                              where transcript.IsCanonical == "true"
                              select new CsqEntry
                              {
                                  Allele      = jsonVariant.GenotypeIndex.ToString(),
                                  Feature     = transcript.TranscriptID,
                                  FeatureType = CsqCommon.TranscriptFeatureType,
                                  Symbol      = transcript.Hgnc,
                                  Consequence = transcript.Consequence == null ? null : string.Join("&", transcript.Consequence)
                              });

                csqs.AddRange(jsonVariant.RegulatoryRegions.Select(regulatoryRegion => new CsqEntry
                {
                    Allele      = jsonVariant.GenotypeIndex.ToString(),
                    Consequence = string.Join("&", regulatoryRegion.Consequence),
                    Feature     = regulatoryRegion.ID,
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
				foreach (var suppAnnotation in altAllele.SuppAnnotations)
				{
					if (suppAnnotation.KeyName != DbSnpKeyName) continue;
					foreach (var s in suppAnnotation.GetStrings("vcf"))
					{
						dbSnp.Add(s);
					}
				}

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
            var phyloP          = new VcfInfoKeyValue("phyloP");

            var suppAnnotationSources = new Dictionary<string, VcfInfoKeyValue>();
            var isSaArrayInfo         = new Dictionary<string, bool>();

            int numAltAlleles = unifiedJson.AnnotatedAlternateAlleles.Count(allele => !allele.IsRecomposedVariant);

            foreach (var alternateAllele in unifiedJson.AnnotatedAlternateAlleles)
            {
                foreach (var sa in alternateAllele.SuppAnnotations)
                {
                    if (!suppAnnotationSources.ContainsKey(sa.KeyName))
                    {
                        suppAnnotationSources[sa.KeyName] = new VcfInfoKeyValue(sa.VcfKeyName);
                        isSaArrayInfo[sa.KeyName] = sa.IsArray;
                    }
                }
            }

            foreach (var kvp in suppAnnotationSources)
            {
                if (isSaArrayInfo[kvp.Key]) continue;
                for (var i = 0; i < numAltAlleles; i++) kvp.Value.Add(null);
            }

            for (var i = 0; i < numAltAlleles; i++)
            {
                alleleFreq1000G.Add(null);
                ancestralAllele.Add(null);
            }

            // understand the number of annotation contains in the whole vcf line
            foreach (var jsonVariant in unifiedJson.AnnotatedAlternateAlleles)
            {
                if (jsonVariant.GenotypeIndex == -1) continue;
                if (jsonVariant.IsReferenceMinor) infoEntries.Add("RefMinor");

                phyloP.Add(jsonVariant.PhylopScore);

                foreach (var sa in jsonVariant.SuppAnnotations)
                {
                    if (sa.IsAlleleSpecific != null && !sa.IsAlleleSpecific.Value) continue;
                    if (sa.KeyName == DbSnpKeyName) continue;
                    if (sa.KeyName == RefMinorKeyName) continue;

                    foreach (var vcfAnnotation in sa.GetStrings("vcf"))
                    {
                        if (string.IsNullOrEmpty(vcfAnnotation)) continue;

                        if (sa.KeyName == OneKgKeyName)
                        {
                            var contents       = vcfAnnotation.Split(';');
                            var freq           = contents[0];
                            var ancestryAllele = string.IsNullOrEmpty(contents[1]) ? null : contents[1];

                            alleleFreq1000G.Add(freq, jsonVariant.GenotypeIndex);
                            ancestralAllele.Add(ancestryAllele, jsonVariant.GenotypeIndex);
                            continue;
                        }

                        if (sa.IsAlleleSpecific != null && sa.IsArray && sa.IsAlleleSpecific.Value)
                        {
                            suppAnnotationSources[sa.KeyName].Add(
                                jsonVariant.GenotypeIndex.ToString(CultureInfo.InvariantCulture) + '|' + vcfAnnotation);
                        }
                        else if (!sa.IsArray)
                        {
                            suppAnnotationSources[sa.KeyName].Add(vcfAnnotation, jsonVariant.GenotypeIndex);
                        }
                    }
                }
            }

            foreach (var value in suppAnnotationSources.Values) infoEntries.Add(value.GetString());

            infoEntries.Add(ancestralAllele.GetString());
            infoEntries.Add(alleleFreq1000G.GetString());
            infoEntries.Add(phyloP.GetString());
        }
    }
}
