using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VariantAnnotation.Algorithms;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.DataStructures.Transcript;

namespace VariantAnnotation.DataStructures.JsonAnnotations
{
    /// <summary>
    /// The JsonVariant represents an individual variant record. While one vcf record 
	/// may contain many alternate alleles and overlap with many genes, this class
	/// represents a single allele and gene combination. This was desired because each
	/// record has a parent that links to the gene and each record represents an
	/// individual alternate allele.
    /// </summary>
    public class JsonVariant : IAnnotatedAlternateAllele
	{
        #region members

	    private readonly IAllele _altAllele;
	    private readonly IVariantFeature _variangFeature;
	    public string VariantId => _altAllele.VariantId;
	    public string VariantType => _altAllele.NirvanaVariantType.ToString();
	    public string ReferenceName => _variangFeature.ReferenceName;
	    public int? ReferenceBegin => _altAllele.Start;
	    public int? ReferenceEnd => _altAllele.End;

		public IList<IAnnotatedSA> SuppAnnotations { get; set; }

		public string PhylopScore { get; internal set; } // 11.612

        public string AncestralAllele { get; set; }

	    public string RefAllele => _variangFeature.IsRefMinor ? _altAllele.AlternateAllele : _altAllele.ReferenceAllele;

	    public int GenotypeIndex => _altAllele.GenotypeIndex;


        public string AltAllele => _variangFeature.IsRefMinor ? null : _altAllele.AlternateAllele;
	    public string SaAltAllele => _altAllele.SuppAltAllele;
	    public bool IsReferenceMinor => _variangFeature.IsRefMinor;
	    public bool IsReference => _variangFeature.IsReference;
	    public bool IsReferenceNoCall => _variangFeature.IsRefNoCall;
        public bool IsIntergenic { get; set; }
        public bool IsRecomposedVariant { get; }

        public IList<IAnnotatedTranscript> RefSeqTranscripts { get; set; }
        public IList<IAnnotatedTranscript> EnsemblTranscripts { get; set; }


	    public ISet<IRegulatoryRegion> RegulatoryRegions { get; }

		public ISet<string> OverlappingGenes { get; }

		public IList<IOverlapTranscript> SvOverlappingTranscripts { get; }

        
        #endregion

        #region stringConstants

        const string AncestralAlleleTag        = "ancestralAllele";
        const string AltAlleleTag              = "altAllele";
        const string RefAlleleTag              = "refAllele";
        const string BeginTag                  = "begin";
        const string EndTag                    = "end";
        const string ChromosomeTag             = "chromosome";
        const string PhylopScoreTag            = "phylopScore";
        const string IsReferenceMinorAlleleTag = "isReferenceMinorAllele";
        const string VariantTypeTag            = "variantType";
        const string VidTag                    = "vid";
        const string RegulatoryRegionsTag      = "regulatoryRegions";
        const string OverlappingGenesTag       = "overlappingGenes";
        const string TranscriptsTag            = "transcripts";
        const string RefseqTag                 = "refSeq";
        const string EnsemblTag                = "ensembl";
        const string OverlappingTranscriptsTag = "overlappingTranscripts";
		
		#endregion


		public sealed class SvOverlapTranscript : IOverlapTranscript
        {
            #region members

            public string TranscriptID { get; }
            public string IsCanonical { get; }
            public string Hgnc { get; }
            public string IsPartialOverlap { get; }

            #endregion

            public void SerializeJson(StringBuilder sb)
            {
                var jsonObject = new JsonObject(sb);

                sb.Append(JsonObject.OpenBrace);
                jsonObject.AddStringValue("transcript", TranscriptID);
                jsonObject.AddStringValue("hgnc", Hgnc);
                jsonObject.AddStringValue("isCanonical", IsCanonical, false);
                jsonObject.AddStringValue("partialOverlap", IsPartialOverlap, false);
                sb.Append(JsonObject.CloseBrace);
            }

            public SvOverlapTranscript(DataStructures.Transcript.Transcript transcript, IAllele altAllele)
            {
                TranscriptID      = TranscriptUtilities.GetTranscriptId(transcript);
                IsCanonical       = transcript.IsCanonical ? "true" : null;
                Hgnc              = transcript.Gene.Symbol;
                var isFullOverlap = altAllele.Start <= transcript.Start && altAllele.End >= transcript.End;
                IsPartialOverlap  = isFullOverlap ? null : "true";
            }

            #region IEquatable methods

            public override int GetHashCode()
            {
                return TranscriptID?.GetHashCode() ?? 0;
            }

            public bool Equals(IOverlapTranscript value)
            {
                if (this == null) throw new NullReferenceException();
                if (value == null) return false;
                if (this == value) return true;
                return TranscriptID == value.TranscriptID;
            }

            #endregion
        }

        public sealed class RegulatoryRegion :  IRegulatoryRegion
        {
            #region members

            public string ID { get; set; }
            public string Type { get; set; }
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
                jsonObject.AddStringValue("type", Type);
                jsonObject.AddStringValues("consequence", Consequence.ToArray());
                sb.Append(JsonObject.CloseBrace);
            }

            public override bool Equals(object obj)
            {
                var other = obj as RegulatoryRegion;
                if (other == null) return false;
                return ID == other.ID; //consequences are not the determinant factor
            }

            // ReSharper disable once NonReadonlyMemberInGetHashCode
            public override int GetHashCode() => ID.GetHashCode();
        }

        public class Transcript : IAnnotatedTranscript
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
            public string HgvsProteinSequenceName { get; set; }
			public string GeneFusion { get; set; }
            public string IsCanonical { get; set; }              // true
            public string PolyPhenPrediction { get; set; }       // benign
            public string PolyPhenScore { get; set; }            // 0.002
            public string ProteinID { get; set; }                // NP_000029.2
            public string ProteinPosition { get; set; }          // 502
            public string SiftPrediction { get; set; }           // deleterious
            public string SiftScore { get; set; }                // 0.01
            public string TranscriptID { get; set; }             // ENSESTT00000083143
			public string BioType {get; set; }
	        public Dictionary<string, string> AdditionalInfo { get; set; }
// proteinCoding	

            #endregion

            #region stringConstants

            const string TranscriptTag         = "transcript";
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
            const string GeneFusionTag         = "geneFusion";
            const string IsCanonicalTag        = "isCanonical";
            const string PolyPhenScoreTag      = "polyPhenScore";
            const string PolyPhenPredictionTag = "polyPhenPrediction";
            const string ProteinIdTag          = "proteinId";
            const string ProteinPosTag         = "proteinPos";
            const string SiftScoreTag          = "siftScore";
            const string SiftPredictionTag     = "siftPrediction";
            const string BioTypeTag            = "bioType";

            #endregion

            public override string ToString()
            {
                var sb = new StringBuilder();
                var jsonObject = new JsonObject(sb);

                sb.Append(JsonObject.OpenBrace);
				jsonObject.AddStringValue(TranscriptTag, TranscriptID);
				jsonObject.AddStringValue(BioTypeTag, BioType);
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
				jsonObject.AddStringValue(GeneFusionTag,GeneFusion,false);
				jsonObject.AddStringValue(IsCanonicalTag, IsCanonical, false);
				jsonObject.AddStringValue(PolyPhenScoreTag, PolyPhenScore, false);
				jsonObject.AddStringValue(PolyPhenPredictionTag, PolyPhenPrediction);
				jsonObject.AddStringValue(ProteinIdTag, ProteinID);
				jsonObject.AddStringValue(ProteinPosTag, ProteinPosition);
				jsonObject.AddStringValue(SiftScoreTag, SiftScore, false);
				jsonObject.AddStringValue(SiftPredictionTag, SiftPrediction);

				if(AdditionalInfo !=null &&AdditionalInfo.Count>0)
	            foreach (var kvp in AdditionalInfo)
	            {
		            jsonObject.AddStringValue(kvp.Key,kvp.Value,false);
	            }

                sb.Append(JsonObject.CloseBrace);
                return sb.ToString();
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        public JsonVariant(IAllele altAllele, IVariantFeature variant)
        {
            _altAllele = altAllele;
            _variangFeature = variant;

            PhylopScore    = altAllele.ConservationScore;
            IsRecomposedVariant = altAllele.IsRecomposedVariant;
	        
            RefSeqTranscripts        = new List<IAnnotatedTranscript>();
            EnsemblTranscripts       = new List<IAnnotatedTranscript>();
            RegulatoryRegions        = new HashSet<IRegulatoryRegion>();
            OverlappingGenes         = new HashSet<string>();
            SvOverlappingTranscripts = new List<IOverlapTranscript>();
            SuppAnnotations          = new List<IAnnotatedSA>();

        }

        /// <summary>
        /// adds a transcript to this variant
        /// </summary>
        public void AddTranscript(Transcript transcript, TranscriptDataSource transcriptDataSource)
        {
            IList<IAnnotatedTranscript> transcripts;

            switch (transcriptDataSource)
            {
                case TranscriptDataSource.Ensembl:
                    transcripts = EnsemblTranscripts;
                    break;
                case TranscriptDataSource.RefSeq:
                    transcripts = RefSeqTranscripts;
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
            jsonObject.AddIntValue(EndTag, ReferenceEnd);
            jsonObject.AddBoolValue(IsReferenceMinorAlleleTag, true, IsReferenceMinor, "true");
			jsonObject.AddStringValue(VariantTypeTag, VariantType);
			jsonObject.AddStringValue(VidTag, VariantId);
            jsonObject.AddBoolValue("isRecomposedVariant", true, IsRecomposedVariant, "true");

            // regulatory regions
            if (RegulatoryRegions.Count > 0) jsonObject.AddObjectValues(RegulatoryRegionsTag, RegulatoryRegions);

			//add SA 

			if (SuppAnnotations.Count > 0) AddSAstoJsonObject(jsonObject);


			// =================
			// Overlapping Genes
			// =================

			if (OverlappingGenes.Count > 0)
            {
                jsonObject.AddStringValues(OverlappingGenesTag, OverlappingGenes.ToArray());
            }

			// =================
			// Overlapping Transcripts
			// =================
			if (SvOverlappingTranscripts.Any())
			{
				jsonObject.AddObjectValues(OverlappingTranscriptsTag, SvOverlappingTranscripts);
			}

			// ==========
			// transcript
			// ==========

			var hasRefSeq = RefSeqTranscripts.Any();
			var hasEnsembl = EnsemblTranscripts.Any();

            if (hasRefSeq || hasEnsembl)
            {
                jsonObject.OpenObject(TranscriptsTag);
                jsonObject.Reset();

                if (hasRefSeq) jsonObject.AddStringValues(RefseqTag, RefSeqTranscripts.Select(t => t.ToString()), false);
                if (hasEnsembl) jsonObject.AddStringValues(EnsemblTag, EnsemblTranscripts.Select(t => t.ToString()), false);

                jsonObject.CloseObject();
            }

            sb.Append(JsonObject.CloseBrace);
            return sb.ToString();
        }

        private void AddSAstoJsonObject(JsonObject jsonObject)
        {
            var saDict = new Dictionary<string, Tuple<bool, List<string>>>();
            foreach (var sa in SuppAnnotations)
            {
                if (!saDict.ContainsKey(sa.KeyName))
                {
                    saDict[sa.KeyName] = new Tuple<bool, List<string>>(sa.IsArray, new List<string>());
                }

                var jsonStrings = sa.GetStrings("json");
                if (jsonStrings != null) saDict[sa.KeyName].Item2.AddRange(jsonStrings);
            }

            foreach (var kvp in saDict)
            {
                if (kvp.Value.Item1)
                {
                    jsonObject.AddStringValues(kvp.Key, kvp.Value.Item2, false);
                }
                else
                {
                    jsonObject.AddStringValue(kvp.Key, kvp.Value.Item2[0], false);
                }
            }
        }

        public void AddSaDataSources(List<ISaDataSource> saDataSources)
	    {
            if (SuppAnnotations == null) SuppAnnotations = new List<IAnnotatedSA>();
	        if (saDataSources.Count == 0) return;

	        foreach (var dataSource in saDataSources)
	        {
                if (dataSource.MatchByAllele && dataSource.AltAllele != SaAltAllele) continue;

                AnnotatedSaItem annotatedSaItem;

                if (!dataSource.MatchByAllele && dataSource.AltAllele == SaAltAllele)
                {
                    annotatedSaItem = new AnnotatedSaItem(dataSource, true);
                    SuppAnnotations.Add(annotatedSaItem);
                    continue;
                }

                annotatedSaItem = new AnnotatedSaItem(dataSource, null);
                SuppAnnotations.Add(annotatedSaItem);
            }
	    }
	}
}
