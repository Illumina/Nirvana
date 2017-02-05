using System.Collections.Generic;
using System.Text;

namespace VariantAnnotation.Interface
{
    public interface IAnnotatedAlternateAllele
    {
        #region members

        // permanent records
        string VariantId { get; }
        string VariantType { get; }

        string ReferenceName { get; }
        int? ReferenceBegin { get; }
        int? ReferenceEnd { get; }
        string RefAllele { get; }
        string AltAllele { get; }

        bool IsReferenceMinor { get; }
        bool IsIntergenic { get; }
        bool IsReference { get; }
        bool IsReferenceNoCall { get; }
        int GenotypeIndex { get; }
        string SaAltAllele { get; }

        // extensible records

        string PhylopScore { get; } // 11.612

        string AncestralAllele { get; set; }
        string EvsCoverage { get; set; }
        string EvsSamples { get; set; }
        string GlobalMinorAllele { get; set; }
        string GlobalMinorAlleleFrequency { get; set; }


        string AlleleFrequencyAll { get; set; }                      // ALL 0.1239
        string AlleleFrequencyAdMixedAmerican { get; set; }          // AMR 0.1239
        string AlleleFrequencyAfrican { get; set; }                  // AFR 0.1239
        string AlleleFrequencyEastAsian { get; set; }                // EAS 0.1239
        string AlleleFrequencyEuropean { get; set; }                 // EUR 0.1239
        string AlleleFrequencySouthAsian { get; set; }               // SAS 0.1239


        string OneKgAlleleNumberAfrican { get; set; }
        string OneKgAlleleNumberAmerican { get; set; }
        string OneKgAlleleNumberAll { get; set; }
        string OneKgAlleleNumberEastAsian { get; set; }
        string OneKgAlleleNumberEuropean { get; set; }
        string OneKgAlleleNumberSouthAsian { get; set; }

        string OneKgAlleleCountAfrican { get; set; }
        string OneKgAlleleCountAmerican { get; set; }
        string OneKgAlleleCountAll { get; set; }
        string OneKgAlleleCountEastAsian { get; set; }
        string OneKgAlleleCountEuropean { get; set; }
        string OneKgAlleleCountSouthAsian { get; set; }


        string EvsAlleleFrequencyAfricanAmerican { get; set; }       // African American 0.1239
        string EvsAlleleFrequencyEuropeanAmerican { get; set; }      // European American 0.1239
        string EvsAlleleFrequencyAll { get; set; }

        string ExacCoverage { get; set; }
        string ExacAlleleFrequencyAfrican { get; set; }
        string ExacAlleleFrequencyAmerican { get; set; }
        string ExacAlleleFrequencyAll { get; set; }
        string ExacAlleleFrequencyEastAsian { get; set; }
        string ExacAlleleFrequencyFinish { get; set; }
        string ExacAlleleFrequencyNonFinish { get; set; }
        string ExacAlleleFrequencyOther { get; set; }
        string ExacAlleleFrequencySouthAsian { get; set; }

        string ExacAlleleNumberAfrican { get; set; }
        string ExacAlleleNumberAmerican { get; set; }
        string ExacAlleleNumberAll { get; set; }
        string ExacAlleleNumberEastAsian { get; set; }
        string ExacAlleleNumberFinish { get; set; }
        string ExacAlleleNumberNonFinish { get; set; }
        string ExacAlleleNumberOther { get; set; }
        string ExacAlleleNumberSouthAsian { get; set; }

        string ExacAlleleCountAfrican { get; set; }
        string ExacAlleleCountAmerican { get; set; }
        string ExacAlleleCountAll { get; set; }
        string ExacAlleleCountEastAsian { get; set; }
        string ExacAlleleCountFinish { get; set; }
        string ExacAlleleCountNonFinish { get; set; }
        string ExacAlleleCountOther { get; set; }
        string ExacAlleleCountSouthAsian { get; set; }

        string[] DbSnpIds { get; set; }



        // -----------
        // transcripts
        // -----------

		IList<IAnnotatedTranscript> RefSeqTranscripts { get; set; }
        IList<IAnnotatedTranscript> EnsemblTranscripts { get; set; }
	    ISet<IRegulatoryRegion> RegulatoryRegions { get; }

        // -----------------
        // overlapping genes
        // -----------------
        ISet<string> OverlappingGenes { get; }

        // -------------------------
        // Supplementary annotations
        // -------------------------

        ISet<IClinVar> ClinVarEntries { get; }
        IList<ICosmic> CosmicEntries { get; }
        IList<ICustomAnnotation> CustomItems { get; }

        // ----------------------------
        // customIntervals
        // ----------------------------
        IList<ICustomInterval> CustomIntervals { get; }

        //------------------------------
        //Overllaping Transcripts for sv
        //------------------------------
        IList<IOverlapTranscript> SvOverlappingTranscripts { get; }


        #endregion
    }

    public interface ICustomInterval : IInterval, IJsonSerializer
    {
        string ReferenceName { get; }
        string Type { get; }
        IDictionary<string, string> StringValues { get; }
        IDictionary<string, string> NonStringValues { get; }
    }

    public interface ISupplementaryInterval
    {
        int Start { get; }
        int End { get; }
        string ReferenceName { get; }
        string AlternateAllele { get; }
        VariantType VariantType { get; }
        string Source { get; }
        IReadOnlyDictionary<string, string> StringValues { get; }

        IReadOnlyDictionary<string, int> IntValues { get; }
        IEnumerable<string> BoolValues { get; }

        IReadOnlyDictionary<string, double> DoubleValues { get; }
        IReadOnlyDictionary<string, double> PopulationFrequencies { get; }
        IReadOnlyDictionary<string, IEnumerable<string>> StringLists { get; }

        string GetJsonContent();
        double OverlapFraction(int begin, int end);
    }

    public enum ReviewStatusEnum
    {
        // ReSharper disable InconsistentNaming
        no_assertion,
        no_criteria,
        single_submitter,
        multiple_submitters,
        multiple_submitters_no_conflict,
        conflicting_interpretations,
        expert_panel,
        practice_guideline
        // ReSharper restore InconsistentNaming
    }

    public interface IClinVar : IJsonSerializer
    {
        #region members

        IEnumerable<string> AlleleOrigins { get; }
        string AltAllele { get; }
        string ID { get; }
        ReviewStatusEnum ReviewStatus { get; }
        string IsAlleleSpecific { get; }
        IEnumerable<string> MedGenIDs { get; }
        IEnumerable<string> OmimIDs { get; }
        IEnumerable<string> OrphanetIDs { get; }
        IEnumerable<string> Phenotypes { get; }
        string Significance { get; }
        IEnumerable<long> PubmedIds { get; }
        long LastUpdatedDate { get; }
        string SaAltAllele { get; }

        #endregion

    }

    public interface ICosmic : IJsonSerializer
    {
        #region members

        string AltAllele { get; }
        string Gene { get; }
        string ID { get; }
        string IsAlleleSpecific { get; }
        IEnumerable<ICosmicStudy> Studies { get; }
        string SaAltAllele { get; }

        #endregion
    }

    public interface ICosmicStudy : IJsonSerializer
    {
        #region members
        string ID { get; }
        string Histology { get; }
        string PrimarySite { get; }
        #endregion

    }

    public interface ICustomAnnotation : IJsonSerializer
    {
        #region members
        string Id { get; }
        string AnnotationType { get; }
        string AltAllele { get; }
        bool IsPositional { get; }
        string IsAlleleSpecific { get; }
        IDictionary<string, string> StringFields { get; }
        IEnumerable<string> BooleanFields { get; }
        #endregion
    }

    public interface IAnnotatedTranscript
    {
        string AminoAcids { get; }
        string CdsPosition { get; }
        string Codons { get; }
        string ComplementaryDnaPosition { get; }
        IEnumerable<string> Consequence { get; }
        string Exons { get; }
        string Introns { get; }
        string Gene { get; }
        string Hgnc { get; }
        string HgvsCodingSequenceName { get; }
        string HgvsProteinSequenceName { get; }
        string GeneFusion { get; set; }
        string IsCanonical { get; }
        string PolyPhenPrediction { get; }
        string PolyPhenScore { get; }
        string ProteinID { get; }
        string ProteinPosition { get; }
        string SiftPrediction { get; }
        string SiftScore { get; }
        string TranscriptID { get; }
    }

    public interface IRegulatoryRegion : IJsonSerializer
    {
        string ID { get; }
        string Type { get; }
        IEnumerable<string> Consequence { get; }
    }

    public interface IOverlapTranscript : IJsonSerializer
    {
        string TranscriptID { get; }
        string IsCanonical { get; }
        string Hgnc { get; }
        string IsPartialOverlap { get; }
    }

    public interface IJsonSerializer
    {
        void SerializeJson(StringBuilder sb);
    }

}
