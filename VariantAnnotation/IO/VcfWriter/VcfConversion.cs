using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.IO.VcfWriter
{
    public sealed class VcfConversion
    {
        private const string DbSnpKeyName        = "dbsnp";
        private const string OneKgKeyName        = "oneKg";
        private const string RefMinorKeyName     = "RefMinor";
        private const string GlobalAlleleKeyName = "globalAllele";

        private readonly StringBuilder _sb             = new StringBuilder();
        private readonly StringBuilder _csqInfoBuilder = new StringBuilder();
        private readonly List<string> _csqtStrings     = new List<string>();
        private readonly List<string> _csqrStrings     = new List<string>();

        public string Convert(IAnnotatedPosition annotatedPosition)
        {
            _sb.Clear();

            var fields = annotatedPosition.Position.VcfFields;

            // add all of the fields before the info field
            for (var vcfIndex = 0; vcfIndex < VcfCommon.IdIndex; vcfIndex++)
            {
                _sb.Append(fields[vcfIndex]);
                _sb.Append('\t');
            }

            // add dbSNP id
            var dbSnpId = ExtractDbId(annotatedPosition);
            _sb.Append(dbSnpId);
            _sb.Append('\t');

            for (var vcfIndex = VcfCommon.IdIndex + 1; vcfIndex < VcfCommon.InfoIndex; vcfIndex++)
            {
                _sb.Append(fields[vcfIndex]);
                _sb.Append('\t');
            }

            AddInfoField(annotatedPosition, _sb);

            // add all of the fields after the info field
            var numColumns = fields.Length;
            for (var vcfIndex = VcfCommon.InfoIndex + 1; vcfIndex < numColumns; vcfIndex++)
            {
                _sb.Append('\t');
                _sb.Append(fields[vcfIndex]);
            }

            return _sb.ToString();
        }

        private static string ExtractDbId(IAnnotatedPosition annotatedPosition)
        {
            var dbSnp = new VcfField();

            var nonDbsnpIds = GetNonDbsnpIds(annotatedPosition.Position.VcfFields[VcfCommon.IdIndex]);

            if (nonDbsnpIds != null) foreach (var nonDbsnpId in nonDbsnpIds) dbSnp.Add(nonDbsnpId);

            foreach (var annotatedVariant in annotatedPosition.AnnotatedVariants)
            {
                foreach (var suppAnnotation in annotatedVariant.SupplementaryAnnotations)
                {
                    if (suppAnnotation.SaDataSource.KeyName != DbSnpKeyName) continue;
                    foreach (var s in suppAnnotation.GetVcfStrings())
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


        private void AddInfoField(IAnnotatedPosition annotatedPosition, StringBuilder sb)
        {
            var infoEntries = new VcfField();
            var infoField = annotatedPosition.Position.InfoData.UpdatedInfoField;

            if (!string.IsNullOrEmpty(infoField))
            {
                infoEntries.Add(infoField);
            }

            ExtractInfo(annotatedPosition, infoEntries);

            infoField = infoEntries.GetString("");

            // remove .
            if (infoField == ".") infoField = "";

            sb.Append(infoField);

            var csqs = new List<CsqEntry>();

            ExtractCsqs(annotatedPosition, csqs);

            if (csqs.Count != 0)
                if (infoField.Length > 0) sb.Append(";");

            // append CSQ tags using delegate from annotator
            sb.Append(GetCsqtAndCsqrVcfInfo(csqs));

            if (csqs.Count == 0 && infoField.Length == 0)
            {
                sb.Append(".");
            }
        }

        private static void ExtractInfo(IAnnotatedPosition annotatedPosition, VcfField infoEntries)
        {
            var alleleFreq1000G = new VcfInfoKeyValue("AF1000G");
            var ancestralAllele = new VcfPositionalInfo("AA");
            var phyloP          = new VcfInfoKeyValue("phyloP");

            var suppAnnotationSources = new Dictionary<string, VcfInfoKeyValue>();
            var isSaArrayInfo = new Dictionary<string, bool>();
            var numInputAltAlleles = annotatedPosition.Position.AltAlleles.Length;

            foreach (var alternateAllele in annotatedPosition.AnnotatedVariants)
            {
                foreach (var sa in alternateAllele.SupplementaryAnnotations)
                {
                    if (!suppAnnotationSources.ContainsKey(sa.SaDataSource.KeyName))
                    {
                        suppAnnotationSources[sa.SaDataSource.KeyName] = new VcfInfoKeyValue(sa.SaDataSource.VcfkeyName);
                        isSaArrayInfo[sa.SaDataSource.KeyName] = sa.SaDataSource.IsArray;
                    }
                }
            }

            foreach (var kvp in suppAnnotationSources)
            {
                if (isSaArrayInfo[kvp.Key]) continue;
                for (var i = 0; i < numInputAltAlleles; i++) kvp.Value.Add(null);
            }

            for (var i = 0; i < numInputAltAlleles; i++)
            {
                alleleFreq1000G.Add(null);
            }

            var inputGenotypeIndex = GetInputGenotypeIndex(annotatedPosition.Position.AltAlleles, annotatedPosition.AnnotatedVariants);

            // understand the number of annotation contains in the whole vcf line
            for (int i = 0; i < annotatedPosition.AnnotatedVariants.Length; i++)
            {
                var annotatedVariant = annotatedPosition.AnnotatedVariants[i];
                var genotypeIndex = inputGenotypeIndex[i] + 1;
                if (annotatedVariant.Variant.IsRefMinor) infoEntries.Add("RefMinor");

                phyloP.Add(annotatedVariant.PhylopScore?.ToString(CultureInfo.InvariantCulture));

                foreach (var sa in annotatedVariant.SupplementaryAnnotations)
                {
                    if (!sa.SaDataSource.MatchByAllele && !sa.IsAlleleSpecific && sa.SaDataSource.KeyName != GlobalAlleleKeyName) continue;
                    if (sa.SaDataSource.KeyName == DbSnpKeyName) continue;
                    if (sa.SaDataSource.KeyName == RefMinorKeyName) continue;

                    foreach (var vcfAnnotation in sa.GetVcfStrings())
                    {
                        if (string.IsNullOrEmpty(vcfAnnotation)) continue;

                        if (sa.SaDataSource.KeyName == OneKgKeyName)
                        {
                            var contents = vcfAnnotation.Split(';');
                            var freq = contents[0];
                            var ancestryAllele = string.IsNullOrEmpty(contents[1]) ? null : contents[1];

                            alleleFreq1000G.Add(freq, genotypeIndex);
                            ancestralAllele.AddValue(ancestryAllele);
                            continue;
                        }

                        if (sa.SaDataSource.IsArray && sa.IsAlleleSpecific)
                        {
                            suppAnnotationSources[sa.SaDataSource.KeyName].Add(
                                genotypeIndex.ToString(CultureInfo.InvariantCulture) + '|' + vcfAnnotation);
                        }
                        else if (!sa.SaDataSource.IsArray)
                        {
                            suppAnnotationSources[sa.SaDataSource.KeyName].Add(vcfAnnotation, genotypeIndex);
                        }
                    }
                }
            }

            foreach (var value in suppAnnotationSources.Values) infoEntries.Add(value.GetString());

            infoEntries.Add(ancestralAllele.GetString());
            infoEntries.Add(alleleFreq1000G.GetString());
            infoEntries.Add(phyloP.GetString());
        }

        private static int[] GetInputGenotypeIndex(string[] positionAltAlleles, IAnnotatedVariant[] annotatedPositionAnnotatedVariants)
        {

            int numAnnotatedVar = annotatedPositionAnnotatedVariants.Length;
            // alt allele is <NON_REF> or . , and this is a refMinor site
            if (positionAltAlleles.Length == 1 && VcfCommon.ReferenceAltAllele.Contains(positionAltAlleles[0]) && numAnnotatedVar == 1)
                return new []{0};

            var inputGenotypeIndex = new int[numAnnotatedVar];
            var annotatedVarIndex  = 0;

            for (var inputIndex = 0; inputIndex < positionAltAlleles.Length && annotatedVarIndex < numAnnotatedVar; inputIndex++)
            {
                if (VcfCommon.NonInformativeAltAllele.Contains(positionAltAlleles[inputIndex])) continue;
                inputGenotypeIndex[annotatedVarIndex] = inputIndex;
                annotatedVarIndex++;
            }

            if (annotatedVarIndex < numAnnotatedVar)
                throw new Exception($"There are unannotated variants! Input alternative alleles: {string.Join(",", positionAltAlleles)}; annotated alleles: {string.Join(",", annotatedPositionAnnotatedVariants.Select(x => x.Variant.AltAllele))}");
            return inputGenotypeIndex;
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

        private static void ExtractCsqs(IAnnotatedPosition unifiedJson, List<CsqEntry> csqs)
        {
            for (int i = 0; i < unifiedJson.AnnotatedVariants.Length; i++)
            {
                var genotypeIndex = i + 1;
                var jsonVariant = unifiedJson.AnnotatedVariants[i];

                csqs.AddRange(
                    jsonVariant.EnsemblTranscripts.Where(x => x.Transcript.IsCanonical)
                        .Select(transcript => new CsqEntry
                        {
                            Allele      = genotypeIndex.ToString(),
                            Feature     = transcript.Transcript.Id.WithVersion,
                            FeatureType = CsqCommon.TranscriptFeatureType,
                            Symbol      = transcript.Transcript.Gene.Symbol,
                            Consequence = transcript.Consequences == null ? null : string.Join("&", transcript.Consequences.Select(ConsequenceUtil.GetConsequence))
                        }));

                csqs.AddRange(from transcript in jsonVariant.RefSeqTranscripts
                              where transcript.Transcript.IsCanonical
                              select new CsqEntry
                              {
                                  Allele      = genotypeIndex.ToString(),
                                  Feature     = transcript.Transcript.Id.WithVersion,
                                  FeatureType = CsqCommon.TranscriptFeatureType,
                                  Symbol      = transcript.Transcript.Gene.Symbol,
                                  Consequence = transcript.Consequences == null ? null : string.Join("&", transcript.Consequences.Select(ConsequenceUtil.GetConsequence))
                              });

                csqs.AddRange(jsonVariant.RegulatoryRegions.Select(regulatoryRegion => new CsqEntry
                {
                    Allele      = genotypeIndex.ToString(),
                    Consequence = string.Join("&", regulatoryRegion.Consequences.Select(ConsequenceUtil.GetConsequence)),
                    Feature     = regulatoryRegion.RegulatoryRegion.Id.WithoutVersion,
                    FeatureType = CsqCommon.RegulatoryFeatureType
                }));
            }
        }
    }
}