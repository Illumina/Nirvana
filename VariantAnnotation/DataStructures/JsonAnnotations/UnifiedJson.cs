using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.DataStructures.JsonAnnotations
{
    public sealed class UnifiedJson : IAnnotatedVariant
    {
        #region members

        // each unified json entry is associated with a variant object from the vcf file
        private readonly VariantFeature _variant;

        // first comes the vcf positional records
        private readonly string _chromosome;
        private readonly int? _referenceBegin;
	    private readonly int? _referenceEnd;
        private readonly string _referenceAllele;
        private readonly string[] _alternateAlleles;
        private readonly string[] _filters;
        private readonly string _quality;

		public string CytogeneticBand { get; private set; }

		// now we place the samples and variant objects
		public string StrandBias { get; }
		public string RecalibratedQuality { get; }
		public string JointSomaticNormalQuality { get; }
		public string CopyNumber { get; }

        internal static bool NeedsVariantComma;

        #endregion

        #region stringConstants

        const string RefAlleleTag          = "refAllele";
        const string PositionTag           = "position";
        const string ChromosomeTag         = "chromosome";
        const string SamplesTag            = "samples";
        const string VariantsTag           = "variants";
        const string StructuralVariantsTag = "structuralVariants";
        const string TrueTag               = "true";

        #endregion

        #region interfaceProperties

        public string ReferenceName => _chromosome;
		public int? ReferenceBegin => _referenceBegin;
		public string ReferenceAllele => _referenceAllele;
	    public IEnumerable<string> AlternateAlleles => _alternateAlleles;

		private readonly List<JsonSample> _samples;
		public IEnumerable<IAnnotatedSample> AnnotatedSamples => _samples;
		public readonly List<JsonVariant> JsonVariants;
		// The following two private variables keep trac of the variant and transcript currently being annotated by NirvanaAnnotationSource.Annotate()
	    private JsonVariant _currJsonVariant;
	    private JsonVariant.Transcript _currTranscript;
		public IEnumerable<IAnnotatedAlternateAllele> AnnotatedAlternateAlleles => JsonVariants;
	    private List<JsonSupplementaryInterval> _supplementaryIntervals;
	    public IEnumerable<IAnnotatedSupplementaryInterval> SupplementaryIntervals => _supplementaryIntervals;

	    public readonly string InfoFromVcf;
	    #endregion

		// constructor
		public UnifiedJson(VariantFeature variant)
        {
            _variant = variant;

            _chromosome               = variant.ReferenceName;
            _referenceBegin           = variant.VcfReferenceBegin;
            _referenceEnd             = variant.VcfReferenceEnd;
            _referenceAllele          = variant.VcfColumns[VcfCommon.RefIndex].ToUpperInvariant();
			_alternateAlleles         = variant.AlternateAlleles[0].NirvanaVariantType == VariantType.translocation_breakend
										? variant.VcfColumns[VcfCommon.AltIndex].Split(',')
										: variant.VcfColumns[VcfCommon.AltIndex].ToUpperInvariant().Split(',');
            _quality                  = variant.VcfColumns[VcfCommon.QualIndex];
            _filters                  = variant.VcfColumns[VcfCommon.FilterIndex].Split(';');
            StrandBias                = variant.StrandBias?.ToString(CultureInfo.InvariantCulture);
            JointSomaticNormalQuality = variant.JointSomaticNormalQuality?.ToString();
            RecalibratedQuality       = variant.RecalibratedQuality?.ToString();
            CopyNumber                = variant.CopyNumber?.ToString();
		    InfoFromVcf               = variant.VcfColumns[VcfCommon.InfoIndex];
            JsonVariants              = new List<JsonVariant>();
            _samples                  = _variant.ExtractSampleInfo();
        }
		public override string ToString()
	    {
            // return if this is a reference site
	        // ReSharper disable once AssignNullToNotNullAttribute
	        if (JsonVariants.Count == 0) return null;

            var sb         = new StringBuilder();
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

			jsonObject.AddStringValue(ChromosomeTag, _chromosome);
			jsonObject.AddStringValue(RefAlleleTag, _referenceAllele);
			jsonObject.AddIntValue(PositionTag, _referenceBegin);
			jsonObject.AddStringValues("ciPos", _variant.CiPos, false);
			jsonObject.AddStringValues("ciEnd", _variant.CiEnd, false);

			jsonObject.AddStringValue("quality", _quality, false);
			jsonObject.AddStringValues("filters", _filters);
			jsonObject.AddStringValues("altAlleles", _alternateAlleles);
			jsonObject.AddStringValue("strandBias", StrandBias, false);
			jsonObject.AddStringValue("jointSomaticNormalQuality", JointSomaticNormalQuality, false);
			jsonObject.AddStringValue("recalibratedQuality", RecalibratedQuality, false);
			jsonObject.AddStringValue("copyNumber", CopyNumber, false);
			CytogeneticBand = _variant.CytogeneticBand;
			jsonObject.AddStringValue("cytogeneticBand", CytogeneticBand);



			if (_samples != null) jsonObject.AddStringValues(SamplesTag, _samples.Select(s => s.ToString()).ToArray(), false);

			if (_supplementaryIntervals != null)
				if (_supplementaryIntervals.Count != 0)
					jsonObject.AddStringValues(StructuralVariantsTag, _supplementaryIntervals.Select(s => s.GetJsonEntry()).ToArray(), false);

			jsonObject.AddStringValues(VariantsTag, JsonVariants.Select(v => v.ToString()).ToArray(), false);

			sb.Append(JsonObject.CloseBrace.ToString());
			return sb.ToString();
		}

		
 
        public static string GetHeader(string annotator, string creationTime, int jsonSchemaVersion, string vepDataVersion, List<DataSourceVersion> dataSourceVersions, string[] sampleNames = null)
		{
			var sb = new StringBuilder();
            var jsonObject = new JsonObject(sb);

            sb.Append("{\"header\":{");
            jsonObject.AddStringValue("annotator", annotator);
            jsonObject.AddStringValue("creationTime", creationTime);
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



        public void AddExonData(TranscriptAnnotation ta, string exonNumber)
	    {
			var altAllele = ta.AlternateAllele;
			if (!altAllele.AlternateAllele.Contains("N"))
			{
				_currTranscript.AminoAcids = GetAlleleString(ta.ReferenceAminoAcids, ta.AlternateAminoAcids);
			}
			
		    _currTranscript.Exons                   = exonNumber;
		    _currTranscript.HgvsProteinSequenceName = ta.HgvsProteinSequenceName;
		
			if (ta.HasValidCdsStart || ta.HasValidCdsEnd)
			{
				_currTranscript.CdsPosition     = GetCdsRangeString(ta);
				_currTranscript.ProteinPosition = GetProtRangeString(ta);
				_currTranscript.Codons          = ta.GetCodonString();
			}
	    }

	    private void FindCorrespondingJsonVariant(VariantAlternateAllele altAllele)
	    {
		    _currJsonVariant = null;
		    foreach (var jsonVariant in JsonVariants)
		    {
			    if (jsonVariant.ReferenceBegin != altAllele.ReferenceBegin) continue;
			    if (jsonVariant.SaAltAllele != altAllele.SuppAltAllele) continue;

			    _currJsonVariant                        = jsonVariant;
			}
		}

	    public void AddFlankingTranscript(Transcript transcript, TranscriptAnnotation ta, string[] consequences)
	    {
			_currTranscript = new JsonVariant.Transcript
			{
				IsCanonical  = transcript.IsCanonical ? TrueTag : null,
				Consequence  = consequences,
				ProteinID    = ta.HasValidCdnaCodingStart ? transcript.ProteinId : null,
				TranscriptID = transcript.StableId,
				Gene         = transcript.GeneStableId,
				Hgnc         = transcript.GeneSymbol
			};

			if (ta.HasValidCdnaStart && ta.HasValidCdnaEnd)
			{
				_currTranscript.ComplementaryDnaPosition = GetCdnaRangeString(ta);
			}

			_currJsonVariant.AddTranscript(_currTranscript, transcript.TranscriptDataSource);
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
            // populating supplementary interval specific fields
            PopulateSuppIntervalFields(variant.GetSupplementaryIntervals());

            foreach (var altAllele in variant.AlternateAlleles)
            {
                var jsonVariant = new JsonVariant(altAllele, variant);
                JsonVariants.Add(jsonVariant);

                // custom intervals are not part of SA as they are a separate data structure
                AddCustomIntervals(altAllele, jsonVariant);

                if (altAllele.SupplementaryAnnotation == null) continue;

                var sa = altAllele.SupplementaryAnnotation;

                AddSaToVariant(sa, jsonVariant);
            }
        }

        private static void AddCustomIntervals(VariantAlternateAllele altAllele, JsonVariant jsonVariant)
	    {
			// adding the custom intervals
		    if (altAllele.CustomIntervals == null) return;
		    jsonVariant.CustomIntervals.Clear();

		    foreach (var custInterval in altAllele.CustomIntervals)
		    {
			    jsonVariant.CustomIntervals.Add(new JsonVariant.CustomInterval(custInterval));
		    }
	    }

		private void PopulateSuppIntervalFields(List<SupplementaryInterval> suppIntervals)
	    {
			if (suppIntervals == null) return;
			_supplementaryIntervals = new List<JsonSupplementaryInterval>();

			foreach (var interval in suppIntervals)
			{
				JsonSupplementaryInterval jsonSuppInterval = new JsonSupplementaryInterval(interval);

				_supplementaryIntervals.Add(jsonSuppInterval);

				//compute reciprocal overlap

				if (_referenceBegin == null || _referenceEnd == null) continue;
				if (_referenceBegin >= _referenceEnd) continue; //do not calculate reciprocal overlap for insertion
				if (interval.Start >= interval.End) continue; //donot compute reciprocal overlap if supp interval is insertion

				var variantInterval = new AnnotationInterval(_referenceBegin.Value + 1, _referenceEnd.Value);

				var intervalOverlap = interval.OverlapFraction(variantInterval.Start, variantInterval.End);
				var variantOverlap = variantInterval.OverlapFraction(interval.Start, interval.End);

				jsonSuppInterval.ReciprocalOverlap = Math.Min(intervalOverlap, variantOverlap);
			}
	    }

	    private static void AddSaToVariant(SupplementaryAnnotation sa,  JsonVariant jsonVariant)
	    {
		    jsonVariant.GlobalMinorAllele = sa.GlobalMinorAllele;
		    jsonVariant.GlobalMinorAlleleFrequency = sa.GlobalMinorAlleleFrequency;

			// adding cosmic
			foreach (var cosmicItem in sa.CosmicItems) jsonVariant.CosmicEntries.Add(cosmicItem);

			// adding ClinVar
			foreach (var clinVarItem in sa.ClinVarItems) jsonVariant.ClinVarEntries.Add(clinVarItem);

			// adding custom items
			foreach (var customItem in sa.CustomItems)
			{
				// we need to check if a custom annotation is allele specific. If so, we match the alt allele.
				if (!customItem.IsPositional
					&& customItem.SaAltAllele != jsonVariant.SaAltAllele)
					continue;
				jsonVariant.CustomItems.Add(new CustomItem(jsonVariant.ReferenceName, 
					jsonVariant.ReferenceBegin ?? 0, 
					jsonVariant.RefAllele,
					jsonVariant.AltAllele, 
					customItem.AnnotationType, 
					customItem.Id,
					customItem.IsPositional,
					customItem.StringFields, 
					customItem.BooleanFields,
					customItem.IsAlleleSpecific));
			}

			// adding allele-specific annotations
			SupplementaryAnnotation.AlleleSpecificAnnotation asa;
			if (sa.AlleleSpecificAnnotations.TryGetValue(jsonVariant.SaAltAllele, out asa))
			{
				var newDbSnp = new List<string>();
				foreach (var dbSnp in asa.DbSnp)
				{
					newDbSnp.Add("rs" + dbSnp);
				}
				jsonVariant.DbSnpIds = newDbSnp.ToArray();
				jsonVariant.AncestralAllele = asa.AncestralAllele;
				jsonVariant.EvsCoverage = asa.EvsCoverage;
				jsonVariant.EvsSamples = asa.NumEvsSamples;
				jsonVariant.ExacCoverage = asa.ExacCoverage > 0 ? asa.ExacCoverage.ToString(CultureInfo.InvariantCulture) : null;

				AddAlleleNumberAndCount(jsonVariant,asa);
				AddAlleleFrequencies(jsonVariant, asa);
			}
		}

	    private static void AddAlleleNumberAndCount(JsonVariant jsonVariant, SupplementaryAnnotation.AlleleSpecificAnnotation asa)
	    {
			jsonVariant.ExacAlleleNumberAfrican     = asa.ExacAfrAn?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.ExacAlleleNumberAmerican    = asa.ExacAmrAn?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.ExacAlleleNumberAll         = asa.ExacAllAn?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.ExacAlleleNumberEastAsian   = asa.ExacEasAn?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.ExacAlleleNumberFinish      = asa.ExacFinAn?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.ExacAlleleNumberNonFinish   = asa.ExacNfeAn?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.ExacAlleleNumberOther       = asa.ExacOthAn?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.ExacAlleleNumberSouthAsian  = asa.ExacSasAn?.ToString(CultureInfo.InvariantCulture);

			jsonVariant.ExacAlleleCountAfrican      = asa.ExacAfrAc?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.ExacAlleleCountAmerican     = asa.ExacAmrAc?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.ExacAlleleCountAll          = asa.ExacAllAc?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.ExacAlleleCountEastAsian    = asa.ExacEasAc?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.ExacAlleleCountFinish       = asa.ExacFinAc?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.ExacAlleleCountNonFinish    = asa.ExacNfeAc?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.ExacAlleleCountOther        = asa.ExacOthAc?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.ExacAlleleCountSouthAsian   = asa.ExacSasAc?.ToString(CultureInfo.InvariantCulture);

			jsonVariant.OneKgAlleleNumberAfrican    = asa.OneKgAfrAn?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.OneKgAlleleNumberAmerican   = asa.OneKgAmrAn?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.OneKgAlleleNumberAll        = asa.OneKgAllAn?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.OneKgAlleleNumberEastAsian  = asa.OneKgEasAn?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.OneKgAlleleNumberEuropean   = asa.OneKgEurAn?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.OneKgAlleleNumberSouthAsian = asa.OneKgSasAn?.ToString(CultureInfo.InvariantCulture);

			jsonVariant.OneKgAlleleCountAfrican     = asa.OneKgAfrAc?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.OneKgAlleleCountAmerican    = asa.OneKgAmrAc?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.OneKgAlleleCountAll         = asa.OneKgAllAc?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.OneKgAlleleCountEastAsian   = asa.OneKgEasAc?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.OneKgAlleleCountEuropean    = asa.OneKgEurAc?.ToString(CultureInfo.InvariantCulture);
			jsonVariant.OneKgAlleleCountSouthAsian  = asa.OneKgSasAc?.ToString(CultureInfo.InvariantCulture);
		}

	    private static void AddAlleleFrequencies(JsonVariant jsonVariant, SupplementaryAnnotation.AlleleSpecificAnnotation asa)
		{

			jsonVariant.AlleleFrequencyAll                 = ComputeFrequency(asa.OneKgAllAn, asa.OneKgAllAc);
			jsonVariant.AlleleFrequencyAfrican             = ComputeFrequency(asa.OneKgAfrAn, asa.OneKgAfrAc);
			jsonVariant.AlleleFrequencyAdMixedAmerican     = ComputeFrequency(asa.OneKgAmrAn, asa.OneKgAmrAc);
			jsonVariant.AlleleFrequencyEastAsian           = ComputeFrequency(asa.OneKgEasAn, asa.OneKgEasAc);
			jsonVariant.AlleleFrequencyEuropean            = ComputeFrequency(asa.OneKgEurAn, asa.OneKgEurAc);
			jsonVariant.AlleleFrequencySouthAsian          = ComputeFrequency(asa.OneKgSasAn, asa.OneKgSasAc);


			jsonVariant.EvsAlleleFrequencyAfricanAmerican  = asa.EvsAfr;
			jsonVariant.EvsAlleleFrequencyEuropeanAmerican = asa.EvsEur;
			jsonVariant.EvsAlleleFrequencyAll              = asa.EvsAll;


			jsonVariant.ExacAlleleFrequencyAfrican         = ComputeFrequency(asa.ExacAfrAn, asa.ExacAfrAc);
			jsonVariant.ExacAlleleFrequencyAmerican        = ComputeFrequency(asa.ExacAmrAn, asa.ExacAmrAc);
			jsonVariant.ExacAlleleFrequencyAll             = ComputeFrequency(asa.ExacAllAn, asa.ExacAllAc);
			jsonVariant.ExacAlleleFrequencyEastAsian       = ComputeFrequency(asa.ExacEasAn, asa.ExacEasAc);
			jsonVariant.ExacAlleleFrequencyFinish          = ComputeFrequency(asa.ExacFinAn, asa.ExacFinAc);
			jsonVariant.ExacAlleleFrequencyNonFinish       = ComputeFrequency(asa.ExacNfeAn, asa.ExacNfeAc);
			jsonVariant.ExacAlleleFrequencyOther           = ComputeFrequency(asa.ExacOthAn, asa.ExacOthAc);
			jsonVariant.ExacAlleleFrequencySouthAsian      = ComputeFrequency(asa.ExacSasAn, asa.ExacSasAc);
		}

	    private static string ComputeFrequency(int? alleleNumber, int? alleleCount)
	    {
		    return alleleNumber != null && alleleNumber.Value > 0 && alleleCount != null
			    ? ((double) alleleCount/alleleNumber.Value).ToString(JsonCommon.FrequencyRoundingFormat)
			    : null;
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
				TranscriptID = transcript.StableId,
				Gene         = transcript.GeneStableId,
				Hgnc         = transcript.GeneSymbol
			};
		}

		public void FinalizeAndAddAnnotationObject(Transcript transcript, TranscriptAnnotation ta, string[] consequences)
	    {
			if (!ta.AlternateAllele.IsStructuralVariant)
			{
				_currTranscript.ComplementaryDnaPosition = GetCdnaRangeString(ta);
				_currTranscript.HgvsCodingSequenceName   = ta.HgvsCodingSequenceName;

				if (ta.HasValidCdnaCodingStart) _currTranscript.ProteinID = transcript.ProteinId;
			}
			else
			{
                _currTranscript.ProteinID = ta.HasValidCdnaCodingStart ? transcript.ProteinId : null;
            }

            _currTranscript.Consequence = consequences;
            _currJsonVariant.AddTranscript(_currTranscript, transcript.TranscriptDataSource);
		}

	    public void AddRegulatoryFeature(RegulatoryFeature regulatoryFeature, VariantAlternateAllele altAllele, string[] consequences)
	    {
			var regulatoryRegion = new JsonVariant.RegulatoryRegion
			{
				ID          = regulatoryFeature.StableId,
				Consequence = consequences
			};
			
			FindCorrespondingJsonVariant(altAllele);
		    _currJsonVariant.RegulatoryRegions.Add(regulatoryRegion);
		}

	    public void AddOverlappingGenes(List<Gene> overlappingGenes, VariantAlternateAllele altAllele)
	    {
			if (!altAllele.IsStructuralVariant) return;
			FindCorrespondingJsonVariant(altAllele);
		    foreach (var gene in overlappingGenes)
		    {
			    _currJsonVariant.OverlappingGenes.Add(gene.Symbol);
		    }
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

            int begin = ta.ComplementaryDnaBegin;
            int end = ta.ComplementaryDnaEnd;

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

            int begin = ta.CodingDnaSequenceBegin;
            int end = ta.CodingDnaSequenceEnd;

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

            int begin = ta.ProteinBegin;
            int end = ta.ProteinEnd;

            if (end < begin) Swap.Int(ref begin, ref end);

            return begin == end ? begin.ToString(CultureInfo.InvariantCulture) : $"{begin}-{end}";
        }
    }
}
