using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;
using ErrorHandling.Exceptions;
using VariantAnnotation.Algorithms;
using VariantAnnotation.DataStructures.Annotation;
using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.DataStructures.Variants;
using VariantAnnotation.FileHandling.VCF;

namespace VariantAnnotation.DataStructures.JsonAnnotations
{
    public sealed class UnifiedJson : IAnnotatedVariant
    {
        #region members        

        private readonly VariantFeature _variant;

        private int? ReferenceEnd => _variant.VcfReferenceEnd;
        public string CytogeneticBand { get; private set; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public string Depth { get; }

		internal static bool NeedsVariantComma;

        // The following two private variables keep track of the variant and transcript currently being annotated by NirvanaAnnotationSource.Annotate()
        private JsonVariant _currJsonVariant;
        private JsonVariant.Transcript _currTranscript;

        #endregion

        #region stringConstants

        const string RefAlleleTag  = "refAllele";
        const string PositionTag   = "position";
        const string ChromosomeTag = "chromosome";
        const string SamplesTag    = "samples";
        const string VariantsTag   = "variants";
        const string TrueTag       = "true";

        #endregion

        #region interfaceProperties

        public IEnumerable<string> AlternateAlleles
            => _variant.AlternateAlleles[0].NirvanaVariantType == VariantType.translocation_breakend
                ? _variant.VcfColumns[VcfCommon.AltIndex].Split(',')
                : _variant.VcfColumns[VcfCommon.AltIndex].ToUpperInvariant().Split(',');

        public IEnumerable<IAnnotatedSample> AnnotatedSamples { get; }

	    public IList<IAnnotatedAlternateAllele> AnnotatedAlternateAlleles { get; } = new List<IAnnotatedAlternateAllele>();

	    public IEnumerable<IAnnotatedSupplementaryInterval> SupplementaryIntervals { get; private set; }

        public string ReferenceName             => _variant.ReferenceName;
        public int? ReferenceBegin              => _variant.VcfReferenceBegin;
        public string ReferenceAllele           => _variant.VcfColumns[VcfCommon.RefIndex].ToUpperInvariant();
        public string InfoFromVcf               => _variant.VcfColumns[VcfCommon.InfoIndex];
        public int? SvEnd                       => _variant.SvEnd;
        public string[] Filters                 => _variant.VcfColumns[VcfCommon.FilterIndex].Split(';');
        public string Quality                   => _variant.VcfColumns[VcfCommon.QualIndex];
        public string StrandBias                => _variant.StrandBias?.ToString();
        public string RecalibratedQuality       => _variant.RecalibratedQuality?.ToString();
        public string JointSomaticNormalQuality => _variant.JointSomaticNormalQuality?.ToString();
        public string CopyNumber                => _variant.CopyNumber?.ToString();
        public bool ColocalizedWithCnv          => _variant.ColocalizedWithCnv;
        public string[] CiPos                   => _variant.CiPos;
        public string[] CiEnd                   => _variant.CiEnd;
        public int? SvLength                    => _variant.SvLength;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public UnifiedJson(VariantFeature variant)
        {
            _variant = variant;
            AnnotatedSamples = variant.ExtractSampleInfo();
        }

        public override string ToString()
        {
            // return if this is a reference site
            // ReSharper disable once AssignNullToNotNullAttribute
	        if (!AnnotatedAlternateAlleles.Any()) return null;

            var sb = new StringBuilder();
            var jsonObject = new JsonObject(sb);

            if (NeedsVariantComma)
            {
                sb.Append(JsonObject.Comma);
                sb.Append('\n');
            }
            else NeedsVariantComma = true;

            // data section
            sb.Append(JsonObject.OpenBrace);

            // ==========
            // positional
            // ==========

			jsonObject.AddStringValue(ChromosomeTag, ReferenceName);
			jsonObject.AddStringValue(RefAlleleTag, ReferenceAllele);
			jsonObject.AddIntValue(PositionTag, ReferenceBegin);
	        if (_variant.IsRepeatExpansion)
	        {
		        jsonObject.AddIntValue("svEnd", _variant.VcfReferenceEnd);
				jsonObject.AddStringValue("repeatUnit", _variant.RepeatUnit);
		        jsonObject.AddIntValue("refRepeatCount", _variant.RefRepeatCount);
			}
				
			jsonObject.AddIntValue("svEnd", SvEnd);
			jsonObject.AddStringValues("ciPos", CiPos, false);
			jsonObject.AddStringValues("ciEnd", CiEnd, false);
			jsonObject.AddIntValue("svLength",SvLength);

			jsonObject.AddStringValue("quality", Quality, false);
			jsonObject.AddStringValues("filters", Filters);
			jsonObject.AddStringValues("altAlleles", AlternateAlleles);
            jsonObject.AddStringValue("strandBias", StrandBias, false);
            jsonObject.AddStringValue("jointSomaticNormalQuality", JointSomaticNormalQuality, false);
            jsonObject.AddStringValue("recalibratedQuality", RecalibratedQuality, false);
            jsonObject.AddStringValue("copyNumber", CopyNumber, false);
			
			jsonObject.AddStringValue("cytogeneticBand", CytogeneticBand);
			jsonObject.AddBoolValue("colocalizedWithCnv",ColocalizedWithCnv,true,TrueTag);

			if (AnnotatedSamples != null) jsonObject.AddStringValues(SamplesTag, AnnotatedSamples.Select(s => s.ToString()).ToArray(), false);

			if (SupplementaryIntervals != null && SupplementaryIntervals.Any())
				AddSuppIntervalToJsonObject(jsonObject);

			jsonObject.AddStringValues(VariantsTag, AnnotatedAlternateAlleles.Select(v => v.ToString()).ToArray(), false);

			sb.Append(JsonObject.CloseBrace.ToString());
			return sb.ToString();
        }

		private void AddSuppIntervalToJsonObject(JsonObject jsonObject)
		{
			var saDict = new Dictionary<string, List<string>>();
			foreach (var si in SupplementaryIntervals)
			{
				if (!saDict.ContainsKey(si.KeyName))
				{
					saDict[si.KeyName] = new List<string>();
				}
				saDict[si.KeyName].AddRange(si.GetStrings("json"));
			}

			foreach (var kvp in saDict)
			{
				jsonObject.AddStringValues(kvp.Key, kvp.Value, false);
			}
		}

		public static string GetHeader(string annotator, string creationTime, string genomeAssembly, int jsonSchemaVersion, string vepDataVersion, List<DataSourceVersion> dataSourceVersions, string[] sampleNames = null)
        {
            var sb = new StringBuilder();
            var jsonObject = new JsonObject(sb);

            sb.Append("{\"header\":{");
            jsonObject.AddStringValue("annotator", annotator);
            jsonObject.AddStringValue("creationTime", creationTime);
            jsonObject.AddStringValue("genomeAssembly", genomeAssembly);
            jsonObject.AddIntValue("schemaVersion", jsonSchemaVersion);
            jsonObject.AddStringValue("dataVersion", vepDataVersion);

            // print our data source versions
            if (dataSourceVersions != null)
            {
                jsonObject.AddObjectValues("dataSources", dataSourceVersions);
            }

            if (sampleNames != null)
            {
                jsonObject.AddStringValues(SamplesTag, sampleNames);
            }

            sb.Append("},\"positions\":[\n");

            return sb.ToString();
        }



        public void AddExonData(TranscriptAnnotation ta, string exonNumber, bool isStructuralVariant)
        {
            var altAllele = ta.AlternateAllele;
            if (!isStructuralVariant && !altAllele.AlternateAllele.Contains("N"))
            {
                _currTranscript.AminoAcids = GetAlleleString(ta.ReferenceAminoAcids, ta.AlternateAminoAcids);
            }

            _currTranscript.Exons = exonNumber;
            _currTranscript.HgvsProteinSequenceName = ta.HgvsProteinSequenceName;

            if (!isStructuralVariant && (ta.HasValidCdsStart || ta.HasValidCdsEnd))
            {
                _currTranscript.CdsPosition = GetCdsRangeString(ta);
                _currTranscript.ProteinPosition = GetProtRangeString(ta);
                _currTranscript.Codons = ta.GetCodonString();
            }
        }

	    private void FindCorrespondingJsonVariant(IAllele altAllele)
	    {
		    _currJsonVariant = null;
		    foreach (var annotatedAllele in AnnotatedAlternateAlleles)
		    {
				var jsonVariant = annotatedAllele as JsonVariant;

		        if (jsonVariant?.ReferenceBegin != altAllele.Start) continue;
                if (jsonVariant.SaAltAllele != altAllele.SuppAltAllele) continue;
                if (jsonVariant.IsRecomposedVariant != altAllele.IsRecomposedVariant) continue;
                _currJsonVariant = jsonVariant;
			}
		}

        public void AddFlankingTranscript(Transcript.Transcript transcript, TranscriptAnnotation ta, string[] consequences)
        {
            _currTranscript = new JsonVariant.Transcript
            {
                IsCanonical  = transcript.IsCanonical ? TrueTag : null,
                Consequence  = consequences,
                ProteinID    = ta.HasValidCdnaCodingStart ? TranscriptUtilities.GetProteinId(transcript) : null,
                TranscriptID = TranscriptUtilities.GetTranscriptId(transcript),
                BioType      = BioTypeUtilities.GetBiotypeDescription(transcript.BioType),
                Gene         = transcript.TranscriptSource == TranscriptDataSource.Ensembl ? transcript.Gene.EnsemblId.ToString() : transcript.Gene.EntrezGeneId.ToString(),
                Hgnc         = transcript.Gene.Symbol
            };

            if (ta.HasValidCdnaStart && ta.HasValidCdnaEnd)
            {
                _currTranscript.ComplementaryDnaPosition = GetCdnaRangeString(ta);
            }

            _currJsonVariant.AddTranscript(_currTranscript, transcript.TranscriptSource);
        }

        public void AddIntergenicVariant(VariantAlternateAllele altAllele)
        {
            // in json, we do not output intergenic_variant since consequences are per transcripts and there are no transcripts for intergenic variants
            FindCorrespondingJsonVariant(altAllele);
            _currJsonVariant.IsIntergenic = true;
        }

        public void AddIntronData(string intronNumber)
        {
            _currTranscript.Introns = intronNumber;
        }

        public void AddProteinChangeEffect(VariantFeature variant)
        {
            if (variant.SiftPrediction != null)
            {
                _currTranscript.SiftPrediction = variant.SiftPrediction;
                _currTranscript.SiftScore = variant.SiftScore;
            }

            if (variant.PolyPhenPrediction != null)
            {
                _currTranscript.PolyPhenPrediction = variant.PolyPhenPrediction;
                _currTranscript.PolyPhenScore = variant.PolyPhenScore;
            }
        }

        public void AddVariantData(VariantFeature variant)
        {
			//add CytoGenetic band
	        CytogeneticBand = variant.CytogeneticBand;

            // populating supplementary interval specific fields
            PopulateSuppIntervalFields(variant.GetSupplementaryIntervals(),variant.IsStructuralVariant);

            foreach (var altAllele in variant.AlternateAlleles)
            {
                var jsonVariant = new JsonVariant(altAllele, variant);

                AnnotatedAlternateAlleles.Add(jsonVariant);

                if (altAllele.SupplementaryAnnotationPosition == null) continue;

                var sa = altAllele.SupplementaryAnnotationPosition;
                jsonVariant.AddSaDataSources(sa);
            }
        }

        private void PopulateSuppIntervalFields(List<IInterimInterval> suppIntervals, bool isStructualVariant)
        {
			if (suppIntervals == null) return;
			var supplementaryIntervals = new List<IAnnotatedSupplementaryInterval>();

			foreach (var interval in suppIntervals)
			{
				if (isStructualVariant &&
					interval.ReportingFor != ReportFor.AllVariants && interval.ReportingFor != ReportFor.StructuralVariants)
					continue;

				if (!isStructualVariant && interval.ReportingFor != ReportFor.AllVariants && interval.ReportingFor != ReportFor.SmallVariants)
					continue;

				var jsonSuppInterval = new JsonSupplementaryInterval(interval);

				supplementaryIntervals.Add(jsonSuppInterval);

				//compute reciprocal overlap

				if ( !isStructualVariant || ReferenceBegin == null || ReferenceEnd == null ) continue;
				if (ReferenceBegin >= ReferenceEnd) continue; //do not calculate reciprocal overlap for insertion
				if (interval.Start >= interval.End) continue; //donot compute reciprocal overlap if supp interval is insertion


				jsonSuppInterval.ReciprocalOverlap = GetReciprocalOverlap(interval.Start, interval.End, ReferenceBegin.Value + 1,
					ReferenceEnd.Value);
			}

			SupplementaryIntervals = supplementaryIntervals;
		}

		private double GetReciprocalOverlap(int start1, int end1, int start2, int end2)
		{
			var overlapStart = Math.Max(start1, start2);
			var overlapEnd = Math.Min(end1, end2);

			var maxLen = Math.Max(end2 - start2 + 1, end1 - start1 + 1);

			return Math.Max(0, (overlapEnd - overlapStart + 1) * 1.0 / maxLen);
		}



		public void CreateAnnotationObject(Transcript.Transcript transcript, VariantAlternateAllele altAllele)
        {
            // while annotating alternate allele, the first output function to be called is AddExonData. 
            // So, we set the current json variant and transcript here.
            // they will subsequently be used in other output functions.
            FindCorrespondingJsonVariant(altAllele);

            if (_currJsonVariant == null)
            {
                throw new GeneralException("Cannot find jsonVariant corresponding to alternate allele");
            }

            _currTranscript = new JsonVariant.Transcript
            {
                IsCanonical  = transcript.IsCanonical ? TrueTag : null,
                TranscriptID = TranscriptUtilities.GetTranscriptId(transcript),
                BioType      = BioTypeUtilities.GetBiotypeDescription(transcript.BioType),
                Gene         = transcript.TranscriptSource == TranscriptDataSource.Ensembl ? transcript.Gene.EnsemblId.ToString() : transcript.Gene.EntrezGeneId.ToString(),
                Hgnc         = transcript.Gene.Symbol
            };
        }

        public void FinalizeAndAddAnnotationObject(Transcript.Transcript transcript, TranscriptAnnotation ta, string[] consequences)
        {
            if (!ta.AlternateAllele.IsStructuralVariant)
            {
                _currTranscript.ComplementaryDnaPosition = GetCdnaRangeString(ta);
                _currTranscript.HgvsCodingSequenceName = ta.HgvsCodingSequenceName;

                if (ta.HasValidCdnaCodingStart) _currTranscript.ProteinID = TranscriptUtilities.GetProteinId(transcript);
            }
            else
            {
                _currTranscript.ProteinID = ta.HasValidCdnaCodingStart ? TranscriptUtilities.GetProteinId(transcript) : null;
                if (ta.GeneFusionAnnotations != null && ta.GeneFusionAnnotations.Count == 1)
                {
                    var sb = new StringBuilder();
                    ta.GeneFusionAnnotations.First().SerializeJson(sb);
                    _currTranscript.GeneFusion = sb.ToString();
                }
                else if (ta.GeneFusionAnnotations != null && ta.GeneFusionAnnotations.Count > 1)
                {
                    throw new Exception("has mutiple gene fusions");
                }
            }

            _currTranscript.Consequence = consequences;
            _currJsonVariant.AddTranscript(_currTranscript, transcript.TranscriptSource);
        }

        public void AddRegulatoryFeature(RegulatoryElement regulatoryFeature, VariantAlternateAllele altAllele, string[] consequences)
        {
            var regulatoryRegion = new JsonVariant.RegulatoryRegion
            {
                ID          = regulatoryFeature.Id.ToString(),
                Type        = regulatoryFeature.Type.ToString(),
                Consequence = consequences
            };

            FindCorrespondingJsonVariant(altAllele);
            _currJsonVariant.RegulatoryRegions.Add(regulatoryRegion);
        }

        public void AddOverlappingGenes(HashSet<string> overlappingGeneSymbols, VariantAlternateAllele altAllele)
        {
            if (!altAllele.IsStructuralVariant) return;
            FindCorrespondingJsonVariant(altAllele);

            foreach (var geneSymbol in overlappingGeneSymbols)
            {
                _currJsonVariant.OverlappingGenes.Add(geneSymbol);
            }
        }

        public void AddOverlappingTranscript(Transcript.Transcript transcript, IAllele altAllele)
        {
            if (!altAllele.IsStructuralVariant) return;
            FindCorrespondingJsonVariant(altAllele);
            var svOverlapTranscript = new JsonVariant.SvOverlapTranscript(transcript, altAllele);
            _currJsonVariant.SvOverlappingTranscripts.Add(svOverlapTranscript);

        }

        // ===============
        // utility methods
        // ===============

        /// <summary>
        /// returns an allele string representation of two alleles
        /// </summary>
        private static string GetAlleleString(string a, string b)
        {
            return a == b ? a : $"{(string.IsNullOrEmpty(a) ? "-" : a)}/{(string.IsNullOrEmpty(b) ? "-" : b)}";
        }

        /// <summary>
        /// returns a range string representation of two integers
        /// </summary>
        private static string GetCdnaRangeString(TranscriptAnnotation ta)
        {
            if (!ta.HasValidCdnaStart && !ta.HasValidCdnaEnd) return null;
            if (!ta.HasValidCdnaStart && ta.HasValidCdnaEnd) return "?-" + ta.ComplementaryDnaEnd;
            if (!ta.HasValidCdnaEnd && ta.HasValidCdnaStart) return ta.ComplementaryDnaBegin + "-?";

            var begin = ta.ComplementaryDnaBegin;
            var end = ta.ComplementaryDnaEnd;

            if (end < begin) Swap.Int(ref begin, ref end);

            return begin == end ? begin.ToString(CultureInfo.InvariantCulture) : $"{begin}-{end}";
        }

        /// <summary>
        /// returns a range string representation of two integers
        /// </summary>
        private static string GetCdsRangeString(TranscriptAnnotation ta)
        {
            if (!ta.HasValidCdsStart && !ta.HasValidCdsEnd) return "";
            if (!ta.HasValidCdsStart && ta.HasValidCdsEnd) return "?-" + ta.CodingDnaSequenceEnd;
            if (!ta.HasValidCdsEnd && ta.HasValidCdsStart) return ta.CodingDnaSequenceBegin + "-?";

            var begin = ta.CodingDnaSequenceBegin;
            var end = ta.CodingDnaSequenceEnd;

            if (end < begin) Swap.Int(ref begin, ref end);

            return begin == end ? begin.ToString(CultureInfo.InvariantCulture) : $"{begin}-{end}";
        }

        /// <summary>
        /// returns a range string representation of two integers
        /// </summary>
        private static string GetProtRangeString(TranscriptAnnotation ta)
        {
            if (!ta.HasValidCdsStart && !ta.HasValidCdsEnd) return "";
            if (!ta.HasValidCdsStart && ta.HasValidCdsEnd) return "?-" + ta.ProteinEnd;
            if (!ta.HasValidCdsEnd && ta.HasValidCdsStart) return ta.ProteinBegin + "-?";

            var begin = ta.ProteinBegin;
            var end = ta.ProteinEnd;

            if (end < begin) Swap.Int(ref begin, ref end);

            return begin == end ? begin.ToString(CultureInfo.InvariantCulture) : $"{begin}-{end}";
        }


        /// <summary>
        /// return the unified json string of gene annotation
        /// </summary>
        public static string FormatGeneAnnotations(List<IGeneAnnotation> annotations)
        {
            if (annotations.Count == 0) return null;

            var geneAnnotations = new Dictionary<string,Dictionary<string,List<IGeneAnnotation>>>();

            foreach (var annotation in annotations)
            {
                if (!geneAnnotations.ContainsKey(annotation.GeneName))
                {
                    geneAnnotations[annotation.GeneName] = new Dictionary<string, List<IGeneAnnotation>>();
                }
                if (!geneAnnotations[annotation.GeneName].ContainsKey(annotation.DataSource))
                {
                    geneAnnotations[annotation.GeneName][annotation.DataSource] = new List<IGeneAnnotation>();
                }
                geneAnnotations[annotation.GeneName][annotation.DataSource].Add(annotation);
            }


            var sb = new StringBuilder();
            var needComma = false;


            sb.Append("\n]");
            sb.Append($"{JsonObject.Comma}\"genes\":[\n");

            
            foreach (var geneAnnotationPair in geneAnnotations)
            {

                if (needComma)
                {
                    sb.Append(JsonObject.Comma);
                    sb.Append('\n');
                }
                else
                {
                    needComma = true;
                }
                sb.Append(JsonObject.OpenBrace);
                var geneSb = new StringBuilder();
                var jsonObject = new JsonObject(geneSb);
                jsonObject.AddStringValue("name",geneAnnotationPair.Key);
                foreach (var kvp in geneAnnotationPair.Value)
                {
                    if (kvp.Value[0].IsArray)
                    {
                        jsonObject.AddStringValues(kvp.Key,kvp.Value.Select(x=>x.ToString()),false);
                        continue;
                    }
                    jsonObject.AddStringValue(kvp.Key,kvp.Value[0].ToString(),false);
                }
                sb.Append(geneSb);
                sb.Append(JsonObject.CloseBrace);

            }
            return sb.ToString();
        }
    }
}
