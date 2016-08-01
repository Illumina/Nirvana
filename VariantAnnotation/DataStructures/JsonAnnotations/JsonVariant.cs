using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ErrorHandling.Exceptions;
using VariantAnnotation.Algorithms;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.JsonAnnotations
{
    /// <summary>
    /// The JsonVariant represents an individual variant record in the
    /// Elasticsearch schema. While one vcf record may contain many alternate
    /// alleles and overlap with many genes, this class represents a single allele
    /// and gene combination. This was desired because each record has a parent that
    /// links to the gene and each record represents an individual alternate allele.
    /// </summary>
    public sealed class JsonVariant : IAnnotatedAlternateAllele
    {
        #region members

        // -----------------
        // position-specific
        // -----------------

        public string VariantId { get; }
        public string VariantType { get; }
		public string ReferenceName { get; }    // 1
        public int? ReferenceBegin { get; }     // 3452643
        public int? ReferenceEnd { get; }
        public Tuple<int, int> CiPos { get; }
        public Tuple<int, int> CiEnd { get; }


        public string PhylopScore { get; set; } // 11.612

        public string AncestralAllele { get; set; }
        public string EvsCoverage { get; set; }
        public string EvsSamples { get; set; }
        public string GlobalMinorAllele { get; set; }
        public string RefAllele { get; }
        public string GlobalMinorAlleleFrequency { get; set; }

        public int GenotypeIndex { get; private set; }

        // ---------------
        // allele-specific
        // ---------------

        public string AltAllele { get; }
        public readonly string SaAltAllele;
        public bool IsReferenceMinor { get; }
        public bool IsReference { get; }
        public bool IsReferenceNoCall { get; }
        public bool IsIntergenic { get; set; }
        public IEnumerable<ITranscript> RefSeqTranscripts => _refSeqTranscripts;
        public IEnumerable<ITranscript> EnsemblTranscripts => _ensemblTranscripts;
        IEnumerable<IClinVar> IAnnotatedAlternateAllele.ClinVarEntries => ClinVarEntries;
        IEnumerable<ICosmic> IAnnotatedAlternateAllele.CosmicEntries => CosmicEntries;
        IEnumerable<ICustomAnnotation> IAnnotatedAlternateAllele.CustomItems => CustomItems;

        IEnumerable<IRegulatoryRegion> IAnnotatedAlternateAllele.RegulatoryRegions => RegulatoryRegions;

        IEnumerable<string> IAnnotatedAlternateAllele.OverlappingGenes => OverlappingGenes;
        public string[] DbSnpIds {  get; set; } // rs6025

        //---------------
        // custom intervals
        //-----------------
        IEnumerable<ICustomInterval> IAnnotatedAlternateAllele.CustomIntervals => CustomIntervals;


        // super populations
        public string AlleleFrequencyAll { get; set; }                      // ALL 0.1239
        public string AlleleFrequencyAdMixedAmerican {  get; set; }          // AMR 0.1239
        public string AlleleFrequencyAfrican {  get; set; }                  // AFR 0.1239
        public string AlleleFrequencyEastAsian {  get; set; }                // EAS 0.1239
        public string AlleleFrequencyEuropean {  get; set; }                 // EUR 0.1239
        public string AlleleFrequencySouthAsian {  get; set; }               // SAS 0.1239


        public string OneKgAlleleNumberAfrican {  get; set; }
        public string OneKgAlleleNumberAmerican {  get; set; }
        public string OneKgAlleleNumberAll {  get; set; }
        public string OneKgAlleleNumberEastAsian {  get; set; }
        public string OneKgAlleleNumberEuropean {  get; set; }
        public string OneKgAlleleNumberSouthAsian {  get; set; }

        public string OneKgAlleleCountAfrican {  get; set; }
        public string OneKgAlleleCountAmerican {  get; set; }
        public string OneKgAlleleCountAll {  get; set; }
        public string OneKgAlleleCountEastAsian {  get; set; }
        public string OneKgAlleleCountEuropean {  get; set; }
        public string OneKgAlleleCountSouthAsian {  get; set; }


        public string EvsAlleleFrequencyAfricanAmerican {  get; set; }       // African American 0.1239
        public string EvsAlleleFrequencyEuropeanAmerican {  get; set; }      // European American 0.1239
        public string EvsAlleleFrequencyAll { get; set; }

        public string ExacCoverage { get; set; }
        public string ExacAlleleFrequencyAfrican { get; set; }
        public string ExacAlleleFrequencyAmerican { get; set; }
        public string ExacAlleleFrequencyAll { get; set; }
        public string ExacAlleleFrequencyEastAsian { get; set; }
        public string ExacAlleleFrequencyFinish { get; set; }
        public string ExacAlleleFrequencyNonFinish { get; set; }
        public string ExacAlleleFrequencyOther { get; set; }
        public string ExacAlleleFrequencySouthAsian { get; set; }

        public string ExacAlleleNumberAfrican { get; set; }
        public string ExacAlleleNumberAmerican { get; set; }
        public string ExacAlleleNumberAll { get; set; }
        public string ExacAlleleNumberEastAsian { get; set; }
        public string ExacAlleleNumberFinish { get; set; }
        public string ExacAlleleNumberNonFinish { get; set; }
        public string ExacAlleleNumberOther { get; set; }
        public string ExacAlleleNumberSouthAsian { get; set; }

        public string ExacAlleleCountAfrican { get; set; }
        public string ExacAlleleCountAmerican { get; set; }
        public string ExacAlleleCountAll { get; set; }
        public string ExacAlleleCountEastAsian { get; set; }
        public string ExacAlleleCountFinish { get; set; }
        public string ExacAlleleCountNonFinish { get; set; }
        public string ExacAlleleCountOther { get; set; }
        public string ExacAlleleCountSouthAsian { get; set; }
        // -----------
        // transcripts
        // -----------

        private readonly List<Transcript> _refSeqTranscripts;
        private readonly List<Transcript> _ensemblTranscripts;

        public HashSet<ClinVarItem> ClinVarEntries { get; }
        public List<CosmicItem> CosmicEntries { get; }
        public List<CustomItem> CustomItems { get; }
        public HashSet<RegulatoryRegion> RegulatoryRegions { get; }
        public HashSet<string> OverlappingGenes { get; }

        //-------------
        // custom intervals
        //-------------
        public List<CustomInterval> CustomIntervals { get; }

        #endregion

		#region stringConstants

		const string AncestralAlleleTag        = "ancestralAllele";
		const string AltAlleleTag              = "altAllele";
		const string RefAlleleTag              = "refAllele";
		const string BeginTag                  = "begin";
		const string EndTag                    = "end";
		const string ChromosomeTag             = "chromosome";
		const string PhylopScoreTag            = "phylopScore";
		const string DbsnpTag                  = "dbsnp";
		const string GlobalMinorAlleleTag      = "globalMinorAllele";
		const string GmafTag                   = "gmaf";
		const string IsReferenceMinorAlleleTag = "isReferenceMinorAllele";
		const string VariantTypeTag            = "variantType";
		const string VidTag                    = "vid";
		const string RegulatoryRegionsTag      = "regulatoryRegions";
		const string ClinVarTag                = "clinVar";
		const string CosmicTag                 = "cosmic";
		const string OverlappingGenesTag       = "overlappingGenes";
		const string TranscriptsTag             = "transcripts";
		const string RefseqTag                 = "refSeq";
		const string EnsemblTag                = "ensembl";

		const string OneKgAllTag = "oneKgAll";
		const string OneKgAfrTag = "oneKgAfr";
		const string OneKgAmrTag = "oneKgAmr";
		const string OneKgEasTag = "oneKgEas";
		const string OneKgEurTag = "oneKgEur";
		const string OneKgSasTag = "oneKgSas";
		const string OneKgAllAnTag = "oneKgAllAn";
		const string OneKgAfrAnTag = "oneKgAfrAn";
		const string OneKgAmrAnTag = "oneKgAmrAn";
		const string OneKgEasAnTag = "oneKgEasAn";
		const string OneKgEurAnTag = "oneKgEurAn";
		const string OneKgSasAnTag = "oneKgSasAn";
		const string OneKgAllAcTag = "oneKgAllAc";
		const string OneKgAfrAcTag = "oneKgAfrAc";
		const string OneKgAmrAcTag = "oneKgAmrAc";
		const string OneKgEasAcTag = "oneKgEasAc";
		const string OneKgEurAcTag = "oneKgEurAc";
		const string OneKgSasAcTag = "oneKgSasAc";

		const string EvsCoverageTag = "evsCoverage";
		const string EvsSamplesTag = "evsSamples";
		const string EvsAllTag = "evsAll";
		const string EvsAfrTag = "evsAfr";
		const string EvsEurTag = "evsEur";

		const string ExacCoverageTag = "exacCoverage";

		const string ExacAllTag = "exacAll";
		const string ExacAfrTag = "exacAfr";
		const string ExacAmrTag = "exacAmr";
		const string ExacEasTag = "exacEas";
		const string ExacFinTag = "exacFin";
		const string ExacNfeTag = "exacNfe";
		const string ExacOthTag = "exacOth";
		const string ExacSasTag = "exacSas";

		const string ExacAllAnTag = "exacAllAn";
		const string ExacAfrAnTag = "exacAfrAn";
		const string ExacAmrAnTag = "exacAmrAn";
		const string ExacEasAnTag = "exacEasAn";
		const string ExacFinAnTag = "exacFinAn";
		const string ExacNfeAnTag = "exacNfeAn";
		const string ExacOthAnTag = "exacOthAn";
		const string ExacSasAnTag = "exacSasAn";

		const string ExacAllAcTag = "exacAllAc";
		const string ExacAfrAcTag = "exacAfrAc";
		const string ExacAmrAcTag = "exacAmrAc";
		const string ExacEasAcTag = "exacEasAc";
		const string ExacFinAcTag = "exacFinAc";
		const string ExacNfeAcTag = "exacNfeAc";
		const string ExacOthAcTag = "exacOthAc";
		const string ExacSasAcTag = "exacSasAc";

		#endregion

		
        public class RegulatoryRegion : IJsonSerializer, IRegulatoryRegion
        {
            #region members
            public string ID { get; set; }
            public IEnumerable<string> Consequence { get; set; }

            #endregion

            /// <summary>
            /// JSON string representation of our regulatory region
            /// </summary>
            public void SerializeJson(StringBuilder sb)
            {
                var jsonObject = new JsonObject(sb);

                sb.Append(JsonObject.OpenBrace);
                jsonObject.AddStringValue("id", ID);
                jsonObject.AddStringValues("consequence", Consequence.ToArray());
                sb.Append(JsonObject.CloseBrace);
            }

            public override bool Equals(object obj)
            {
                var other = obj as RegulatoryRegion;

                if (other == null) return false;

                return ID == other.ID; //consequences are not the determinant factor

            }

            public override int GetHashCode()
            {
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                return FowlerNollVoPrimeHash.ComputeHash(ID);
            }


        }

        public class Transcript : ITranscript
        {
            #region members

            public string AminoAcids { get; set; }               // A/ASA
            public string CdsPosition { get; set; }              // 1504-1505
            public string Codons { get; set; }                   // gcc/gCTTCTGcc
            public string ComplementaryDnaPosition { get; set; } // 1601-1602
            public IEnumerable<string> Consequence { get; set; }            // stop_gained
            public string Exons { get; set; }                    // 2-3/7
            public string Introns { get; set; }
            public string Gene { get; set; }                     // ENSESTG00000032903
            public string Hgnc { get; set; }                     // OR4F5
            public string HgvsCodingSequenceName { get; set; }   // NM_001127612.1:c.382C>T
            public string HgvsProteinSequenceName { get; set; }  // NP_000271.1:p.Arg128Cys
            public string IsCanonical { get; set; }              // true
            public string PolyPhenPrediction { get; set; }       // benign
            public string PolyPhenScore { get; set; }            // 0.002
            public string ProteinID { get; set; }                // NP_000029.2
            public string ProteinPosition { get; set; }          // 502
            public string SiftPrediction { get; set; }           // deleterious
            public string SiftScore { get; set; }                // 0.01
            public string TranscriptID { get; set; }             // ENSESTT00000083143

            #endregion
			#region stringConstants
			const string TranscriptTag= "transcript";
			const string AminoAcidsTag         = "aminoAcids";
			const string CdnaPosTag            = "cDnaPos";
			const string CodonsTag             = "codons";
			const string CdsPosTag             = "cdsPos";
			const string ExonsTag              = "exons";
			const string IntronsTag            = "introns";
			const string GeneIdTag             = "geneId";
			const string HgncTag               = "hgnc";
			const string ConsequenceTag        = "consequence";
			const string HgvscTag              = "hgvsc";
			const string HgvspTag              = "hgvsp";
			const string IsCanonicalTag        = "isCanonical";
			const string PolyPhenScoreTag      = "polyPhenScore";
			const string PolyPhenPredictionTag = "polyPhenPrediction";
			const string ProteinIdTag          = "proteinId";
			const string ProteinPosTag         = "proteinPos"; 
			const string SiftScoreTag          = "siftScore";
			const string SiftPredictionTag     = "siftPrediction";
#endregion

			public override string ToString()
            {
                var sb = new StringBuilder();
                var jsonObject = new JsonObject(sb);

                sb.Append(JsonObject.OpenBrace);
				jsonObject.AddStringValue(TranscriptTag, TranscriptID);
				jsonObject.AddStringValue(AminoAcidsTag, AminoAcids);
				jsonObject.AddStringValue(CdnaPosTag, ComplementaryDnaPosition);
				jsonObject.AddStringValue(CodonsTag, Codons);
				jsonObject.AddStringValue(CdsPosTag, CdsPosition);
				jsonObject.AddStringValue(ExonsTag, Exons);
				jsonObject.AddStringValue(IntronsTag, Introns);
				jsonObject.AddStringValue(GeneIdTag, Gene);
				jsonObject.AddStringValue(HgncTag, Hgnc);
				jsonObject.AddStringValues(ConsequenceTag, Consequence?.ToArray());
				jsonObject.AddStringValue(HgvscTag, HgvsCodingSequenceName);
				jsonObject.AddStringValue(HgvspTag, HgvsProteinSequenceName);
				jsonObject.AddStringValue(IsCanonicalTag, IsCanonical, false);
				jsonObject.AddStringValue(PolyPhenScoreTag, PolyPhenScore, false);
				jsonObject.AddStringValue(PolyPhenPredictionTag, PolyPhenPrediction);
				jsonObject.AddStringValue(ProteinIdTag, ProteinID);
				jsonObject.AddStringValue(ProteinPosTag, ProteinPosition);
				jsonObject.AddStringValue(SiftScoreTag, SiftScore, false);
				jsonObject.AddStringValue(SiftPredictionTag, SiftPrediction);

                sb.Append(JsonObject.CloseBrace);
                return sb.ToString();
            }
        }

        //-------
        // custom intervals
        //--------

        // constructor
        private JsonVariant()
        {
            _refSeqTranscripts  = new List<Transcript>();
            _ensemblTranscripts = new List<Transcript>();
            ClinVarEntries      = new HashSet<ClinVarItem>();
            CosmicEntries       = new List<CosmicItem>();
            CustomItems         = new List<CustomItem>();
            RegulatoryRegions   = new HashSet<RegulatoryRegion>();
            CustomIntervals     = new List<CustomInterval>();
            OverlappingGenes    = new HashSet<string>();
        }

        private JsonVariant(VariantAlternateAllele altAllele) : this()
        {
            VariantId = altAllele.VariantId;
            VariantType = altAllele.NirvanaVariantType.ToString();
            ReferenceBegin = altAllele.ReferenceBegin;
            ReferenceEnd = altAllele.ReferenceEnd;
            RefAllele = altAllele.ReferenceAllele;
            AltAllele = altAllele.AlternateAllele;
            SaAltAllele = altAllele.SuppAltAllele;
            GenotypeIndex = altAllele.GenotypeIndex;
        }

        public JsonVariant(VariantAlternateAllele altAllele, VariantFeature variant) : this(altAllele)
        {
            CiPos = variant.CiPos == null ? null : new Tuple<int, int>(Convert.ToInt32(variant.CiPos[0]), Convert.ToInt32(variant.CiPos[1]));
            CiEnd = variant.CiEnd == null ? null : new Tuple<int, int>(Convert.ToInt32(variant.CiEnd[0]), Convert.ToInt32(variant.CiEnd[1]));
            IsReferenceMinor = variant.IsRefMinor;
            IsReference = variant.IsReference;
            IsReferenceNoCall = variant.IsRefNoCall;
            PhylopScore = altAllele.ConservationScore;
            ReferenceName = variant.ReferenceName;

            // change the ref and alternate allele for ref minor
            RefAllele = variant.IsRefMinor ? altAllele.AlternateAllele : altAllele.ReferenceAllele;
            AltAllele = variant.IsRefMinor ? null : altAllele.AlternateAllele;
        }


        /// <summary>
        /// adds a transcript to this variant
        /// </summary>
        public void AddTranscript(Transcript transcript, TranscriptDataSource transcriptDataSource)
        {
            List<Transcript> transcripts;

            switch (transcriptDataSource)
            {
                case TranscriptDataSource.Ensembl:
                    transcripts = _ensemblTranscripts;
                    break;
                case TranscriptDataSource.RefSeq:
                    transcripts = _refSeqTranscripts;
                    break;
                default:
                    throw new GeneralException($"Found a transcript ({transcript.TranscriptID}) with an unexpected transcript data source ({transcriptDataSource})");
            }

            transcripts.Add(transcript);
        }

        /// <summary>
		/// returns a string representation of our variant
		/// </summary>
		public override string ToString()
        {
            var sb = new StringBuilder();
            var jsonObject = new JsonObject(sb);

            // data section
            sb.Append(JsonObject.OpenBrace);

            // ==========
            // positional
            // ==========

            jsonObject.AddStringValue(AncestralAlleleTag, AncestralAllele);

            if (!IsReferenceMinor)
            {
	            jsonObject.AddStringValue(AltAlleleTag, string.IsNullOrEmpty(AltAllele) ? "-" : AltAllele);
                jsonObject.AddStringValue(RefAlleleTag, string.IsNullOrEmpty(RefAllele) ? "-" : RefAllele);
            }
            else
            {
                jsonObject.AddStringValue(RefAlleleTag, string.IsNullOrEmpty(RefAllele) ? "-" : RefAllele);
            }

            jsonObject.AddIntValue(BeginTag, ReferenceBegin);
			jsonObject.AddStringValue(ChromosomeTag, ReferenceName);

			jsonObject.AddStringValue(PhylopScoreTag , PhylopScore, false);
            jsonObject.AddStringValues(DbsnpTag, DbSnpIds);
            jsonObject.AddIntValue(EndTag, ReferenceEnd);
			jsonObject.AddStringValue(GlobalMinorAlleleTag, GlobalMinorAllele);
			jsonObject.AddStringValue(GmafTag, GlobalMinorAlleleFrequency, false);
            jsonObject.AddBoolValue(IsReferenceMinorAlleleTag, true, IsReferenceMinor, "true");
			jsonObject.AddStringValue(VariantTypeTag, VariantType);
			jsonObject.AddStringValue(VidTag, VariantId);

            // regulatory regions
            if (RegulatoryRegions.Count > 0) jsonObject.AddObjectValues(RegulatoryRegionsTag, RegulatoryRegions);

            // ClinVar & COSMIC
            if (ClinVarEntries.Count > 0) jsonObject.AddObjectValues(ClinVarTag, ClinVarEntries);
            if (CosmicEntries.Count > 0) jsonObject.AddObjectValues(CosmicTag, CosmicEntries);
            // Custom annotations
            if (CustomItems.Count > 0)
            {
                var customGroups = new Dictionary<string, List<CustomItem>>();
                foreach (var customItem in CustomItems)
                {
                    var type = customItem.AnnotationType;
                    if (customGroups.ContainsKey(type))
                        customGroups[type].Add(customItem);
                    else
                    {
                        customGroups[type] = new List<CustomItem> { customItem };
                    }
                }
                foreach (var customGroup in customGroups)
                {
                    jsonObject.AddObjectValues(customGroup.Key, customGroup.Value);
                }

            }
            // Custom Intervals
            // if (CustomIntervals.Count > 0) jsonObject.AddObjectValues(CustomIntervals[0].Type, CustomIntervals);
            if (CustomIntervals.Count > 0)
            {
                var intervalGroups = new Dictionary<string, List<CustomInterval>>();
                foreach (var customInterval in CustomIntervals)
                {
                    var type = customInterval.Type;
                    if (intervalGroups.ContainsKey(type))
                        intervalGroups[type].Add(customInterval);
                    else
                    {
                        intervalGroups[type] = new List<CustomInterval> { customInterval };
                    }
                }
                foreach (var intervalGroup in intervalGroups)
                {
                    jsonObject.AddObjectValues(intervalGroup.Key, intervalGroup.Value);
                }
            }

            // =================
            // Overlapping Genes
            // =================

            if (OverlappingGenes.Count > 0)
            {
                jsonObject.AddStringValues(OverlappingGenesTag, OverlappingGenes.ToArray());
            }

            // ==========
            // transcript
            // ==========

            bool hasRefSeq = _refSeqTranscripts.Count > 0;
            bool hasEnsembl = _ensemblTranscripts.Count > 0;

            if (hasRefSeq || hasEnsembl)
            {
                jsonObject.OpenObject(TranscriptsTag);
                jsonObject.Reset();

                if (hasRefSeq) jsonObject.AddStringValues(RefseqTag, _refSeqTranscripts.Select(t => t.ToString()), false);
                if (hasEnsembl) jsonObject.AddStringValues(EnsemblTag, _ensemblTranscripts.Select(t => t.ToString()), false);

                jsonObject.CloseObject();
            }

            // =======
            // allelic
            // =======

            jsonObject.Reset(true);

			jsonObject.AddStringValue(OneKgAllTag, AlleleFrequencyAll, false);
			jsonObject.AddStringValue(OneKgAfrTag, AlleleFrequencyAfrican, false);
			jsonObject.AddStringValue(OneKgAmrTag, AlleleFrequencyAdMixedAmerican, false);
			jsonObject.AddStringValue(OneKgEasTag, AlleleFrequencyEastAsian, false);
			jsonObject.AddStringValue(OneKgEurTag, AlleleFrequencyEuropean, false);
			jsonObject.AddStringValue(OneKgSasTag, AlleleFrequencySouthAsian, false);


			jsonObject.AddStringValue(OneKgAllAnTag, OneKgAlleleNumberAll, false);
			jsonObject.AddStringValue(OneKgAfrAnTag, OneKgAlleleNumberAfrican, false);
			jsonObject.AddStringValue(OneKgAmrAnTag, OneKgAlleleNumberAmerican, false);
			jsonObject.AddStringValue(OneKgEasAnTag, OneKgAlleleNumberEastAsian, false);
			jsonObject.AddStringValue(OneKgEurAnTag, OneKgAlleleNumberEuropean, false);
			jsonObject.AddStringValue(OneKgSasAnTag, OneKgAlleleNumberSouthAsian, false);

			jsonObject.AddStringValue(OneKgAllAcTag, OneKgAlleleCountAll, false);
			jsonObject.AddStringValue(OneKgAfrAcTag, OneKgAlleleCountAfrican, false);
			jsonObject.AddStringValue(OneKgAmrAcTag, OneKgAlleleCountAmerican, false);
			jsonObject.AddStringValue(OneKgEasAcTag, OneKgAlleleCountEastAsian, false);
			jsonObject.AddStringValue(OneKgEurAcTag, OneKgAlleleCountEuropean, false);
			jsonObject.AddStringValue(OneKgSasAcTag, OneKgAlleleCountSouthAsian, false);


			jsonObject.AddStringValue(EvsCoverageTag, EvsCoverage, false);
			jsonObject.AddStringValue(EvsSamplesTag, EvsSamples, false);
			jsonObject.AddStringValue(EvsAllTag, EvsAlleleFrequencyAll, false);
            jsonObject.AddStringValue(EvsAfrTag, EvsAlleleFrequencyAfricanAmerican, false);
            jsonObject.AddStringValue(EvsEurTag, EvsAlleleFrequencyEuropeanAmerican, false);


			jsonObject.AddStringValue(ExacCoverageTag, ExacCoverage , false);
			jsonObject.AddStringValue(ExacAllTag, ExacAlleleFrequencyAll, false);
			jsonObject.AddStringValue(ExacAfrTag, ExacAlleleFrequencyAfrican, false);
			jsonObject.AddStringValue(ExacAmrTag, ExacAlleleFrequencyAmerican, false);
			jsonObject.AddStringValue(ExacEasTag, ExacAlleleFrequencyEastAsian, false);
			jsonObject.AddStringValue(ExacFinTag, ExacAlleleFrequencyFinish, false);
			jsonObject.AddStringValue(ExacNfeTag, ExacAlleleFrequencyNonFinish, false);
			jsonObject.AddStringValue(ExacOthTag, ExacAlleleFrequencyOther, false);
			jsonObject.AddStringValue(ExacSasTag, ExacAlleleFrequencySouthAsian, false);

			jsonObject.AddStringValue(ExacAllAnTag, ExacAlleleNumberAll, false);
			jsonObject.AddStringValue(ExacAfrAnTag, ExacAlleleNumberAfrican, false);
			jsonObject.AddStringValue(ExacAmrAnTag, ExacAlleleNumberAmerican, false);
			jsonObject.AddStringValue(ExacEasAnTag, ExacAlleleNumberEastAsian, false);
			jsonObject.AddStringValue(ExacFinAnTag, ExacAlleleNumberFinish, false);
			jsonObject.AddStringValue(ExacNfeAnTag, ExacAlleleNumberNonFinish, false);
			jsonObject.AddStringValue(ExacOthAnTag, ExacAlleleNumberOther, false);
			jsonObject.AddStringValue(ExacSasAnTag, ExacAlleleNumberSouthAsian, false);

			jsonObject.AddStringValue(ExacAllAcTag, ExacAlleleCountAll, false);
			jsonObject.AddStringValue(ExacAfrAcTag, ExacAlleleCountAfrican, false);
			jsonObject.AddStringValue(ExacAmrAcTag, ExacAlleleCountAmerican, false);
			jsonObject.AddStringValue(ExacEasAcTag, ExacAlleleCountEastAsian, false);
			jsonObject.AddStringValue(ExacFinAcTag, ExacAlleleCountFinish, false);
			jsonObject.AddStringValue(ExacNfeAcTag, ExacAlleleCountNonFinish, false);
			jsonObject.AddStringValue(ExacOthAcTag, ExacAlleleCountOther, false);
			jsonObject.AddStringValue(ExacSasAcTag, ExacAlleleCountSouthAsian, false);
            sb.Append(JsonObject.CloseBrace);
            return sb.ToString();
        }
		

  

        public class CustomInterval : DataStructures.CustomInterval, IJsonSerializer
        {
            public CustomInterval(DataStructures.CustomInterval customInterval)
            {
                Start = customInterval.Start;
                End = customInterval.End;
                ReferenceName = customInterval.ReferenceName;
                Type = customInterval.Type;
                StringValues = customInterval.StringValues;
                NonStringValues = customInterval.NonStringValues;
            }

            public void SerializeJson(StringBuilder sb)
            {
                var jsonObject = new JsonObject(sb);

                sb.Append(JsonObject.OpenBrace);
                jsonObject.AddStringValue("Start", Start.ToString(), false);
                jsonObject.AddStringValue("End", End.ToString(), false);
                // jsonObject.AddStringValue("ReferenceName", ReferenceName);//should be a quoted string
                // jsonObject.AddStringValue("Type",Type);//should be a quoted string

                if (StringValues != null)
                {
                    foreach (var kvp in StringValues)
                    {
                        jsonObject.AddStringValue(kvp.Key, kvp.Value);
                    }
                }

                if (NonStringValues != null)
                {
                    foreach (var kvp in NonStringValues)
                    {
                        jsonObject.AddStringValue(kvp.Key, kvp.Value, false);
                    }
                }

                sb.Append(JsonObject.CloseBrace);
            }
        }
    }
}
