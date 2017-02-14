using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;
using ErrorHandling.Exceptions;
using VariantAnnotation.Algorithms;

namespace VariantAnnotation.DataStructures.JsonAnnotations
{
    public sealed class UnifiedJson : IAnnotatedVariant
    {
        #region members        

        // first comes the vcf positional records
	    private readonly int? _referenceEnd;
        public string[] Filters { get; }
        public string Quality { get; }

        public string CytogeneticBand { get; private set; }

        // now we place the samples and variant objects
        public string StrandBias { get; }
        public string RecalibratedQuality { get; }
        public string JointSomaticNormalQuality { get; }
        public string CopyNumber { get; }
        public string Depth { get; }
        public bool ColocalizedWithCnv { get; }
	    public string[] CiPos { get; }
	    public string[] CiEnd { get; }
	    public int? SvLength { get; }

		internal static bool NeedsVariantComma;

        #endregion

        #region stringConstants

        const string RefAlleleTag = "refAllele";
        const string PositionTag = "position";
        const string ChromosomeTag = "chromosome";
        const string SamplesTag = "samples";
        const string VariantsTag = "variants";
        const string StructuralVariantsTag = "structuralVariants";
        const string TrueTag = "true";

        #endregion

        #region interfaceProperties

        public string ReferenceName { get; }
		public int? ReferenceBegin { get; }
		public string ReferenceAllele { get; }
	    public IEnumerable<string> AlternateAlleles { get; }

		public IEnumerable<IAnnotatedSample> AnnotatedSamples { get; }

	    public IList<IAnnotatedAlternateAllele> AnnotatedAlternateAlleles { get; }

	    public IEnumerable<IAnnotatedSupplementaryInterval> SupplementaryIntervals { get; private set; }

	    public readonly string InfoFromVcf;

		// The following two private variables keep track of the variant and transcript currently being annotated by NirvanaAnnotationSource.Annotate()
		private JsonVariant _currJsonVariant;
		private JsonVariant.Transcript _currTranscript;
	    #endregion

		// constructor
		public UnifiedJson(VariantFeature variant)
		{
			ReferenceName             = variant.ReferenceName;
			ReferenceBegin            = variant.VcfReferenceBegin;
			_referenceEnd             = variant.VcfReferenceEnd;
			ReferenceAllele           = variant.VcfColumns[VcfCommon.RefIndex].ToUpperInvariant();
			AlternateAlleles          = variant.AlternateAlleles[0].NirvanaVariantType == VariantType.translocation_breakend
										? variant.VcfColumns[VcfCommon.AltIndex].Split(',')
										: variant.VcfColumns[VcfCommon.AltIndex].ToUpperInvariant().Split(',');
			Quality                   = variant.VcfColumns[VcfCommon.QualIndex];
			Filters                   = variant.VcfColumns[VcfCommon.FilterIndex].Split(';');
			StrandBias                = variant.StrandBias?.ToString(CultureInfo.InvariantCulture);
			JointSomaticNormalQuality = variant.JointSomaticNormalQuality?.ToString();
			RecalibratedQuality       = variant.RecalibratedQuality?.ToString();
			CopyNumber                = variant.CopyNumber?.ToString();
			InfoFromVcf               = variant.VcfColumns[VcfCommon.InfoIndex];
			AnnotatedAlternateAlleles             = new List<IAnnotatedAlternateAllele>();
			AnnotatedSamples          = variant.ExtractSampleInfo();
			ColocalizedWithCnv        = variant.ColocalizedWithCnv;
			CiPos                     = variant.CiPos;
			CiEnd                     = variant.CiEnd;
			SvLength                  = variant.SvLength;
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
					jsonObject.AddStringValues(StructuralVariantsTag, SupplementaryIntervals.Select(s => s.ToString()).ToArray(), false);

			jsonObject.AddStringValues(VariantsTag, AnnotatedAlternateAlleles.Select(v => v.ToString()).ToArray(), false);

			sb.Append(JsonObject.CloseBrace.ToString());
			return sb.ToString();
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

	    private void FindCorrespondingJsonVariant(VariantAlternateAllele altAllele)
	    {
		    _currJsonVariant = null;
		    foreach (var annotatedAllele in AnnotatedAlternateAlleles)
		    {
				var jsonVariant = annotatedAllele as JsonVariant;

		        if (jsonVariant?.ReferenceBegin != altAllele.Start) continue;
                if (jsonVariant.SaAltAllele != altAllele.SuppAltAllele) continue;

                _currJsonVariant = jsonVariant;
			}
		}

        public void AddFlankingTranscript(Transcript transcript, TranscriptAnnotation ta, string[] consequences)
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
            PopulateSuppIntervalFields(variant.GetSupplementaryIntervals());

            foreach (var altAllele in variant.AlternateAlleles)
            {
                var jsonVariant = new JsonVariant(altAllele, variant);

                AnnotatedAlternateAlleles.Add(jsonVariant);

                // custom intervals are not part of SA as they are a separate data structure
                AddCustomIntervals(altAllele, jsonVariant);

                if (altAllele.SupplementaryAnnotationPosition == null) continue;

                var sa = altAllele.SupplementaryAnnotationPosition;

                sa.AddSaPositionToVariant(jsonVariant);
            }
        }

        private static void AddCustomIntervals(VariantAlternateAllele altAllele, JsonVariant jsonVariant)
        {
            // adding the custom intervals
            if (altAllele.CustomIntervals == null) return;
            jsonVariant.CustomIntervals.Clear();

            foreach (var custInterval in altAllele.CustomIntervals)
            {
			    jsonVariant.CustomIntervals.Add(custInterval);
            }
        }

        private void PopulateSuppIntervalFields(List<ISupplementaryInterval> suppIntervals)
        {
            if (suppIntervals == null) return;
			var supplementaryIntervals = new List<JsonSupplementaryInterval>();

            foreach (var interval in suppIntervals)
            {
                var jsonSuppInterval = new JsonSupplementaryInterval(interval);

				supplementaryIntervals.Add(jsonSuppInterval);

                //compute reciprocal overlap

				if (ReferenceBegin == null || _referenceEnd == null) continue;
				if (ReferenceBegin >= _referenceEnd) continue; //do not calculate reciprocal overlap for insertion
                if (interval.Start >= interval.End) continue; //donot compute reciprocal overlap if supp interval is insertion

				var variantInterval = new AnnotationInterval(ReferenceBegin.Value + 1, _referenceEnd.Value);

                var intervalOverlap = interval.OverlapFraction(variantInterval.Start, variantInterval.End);
                var variantOverlap = variantInterval.OverlapFraction(interval.Start, interval.End);

                jsonSuppInterval.ReciprocalOverlap = Math.Min(intervalOverlap, variantOverlap);
            }

			SupplementaryIntervals = supplementaryIntervals;
        }



        public void CreateAnnotationObject(Transcript transcript, VariantAlternateAllele altAllele)
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

        public void FinalizeAndAddAnnotationObject(Transcript transcript, TranscriptAnnotation ta, string[] consequences)
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

        public void AddOverlappingTranscript(Transcript transcript, VariantAlternateAllele altAllele)
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
        public static string GetGeneAnnotation(List<string> annotations, string description)
        {
            if (annotations.Count == 0) return null;
            var sb = new StringBuilder();
            var needComma = false;


            sb.Append("\n]");
            sb.Append($"{JsonObject.Comma}\"{description}\":[\n");
            foreach (var annotation in annotations)
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

                sb.Append(annotation);
            }
            return sb.ToString();
        }
    }
}
