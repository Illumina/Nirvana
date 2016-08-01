using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    public interface IClinVar
    {
        string AlleleOrigin { get; }   
        string AltAllele { get; }    
        string GeneReviewsID { get; } 
        string ID { get; }        
        ReviewStatus ReviewStatus { get; }
        string IsAlleleSpecific { get; } 
        string MedGenID { get; }     
        string OmimID { get; }    
        string OrphanetID { get; }
        string Phenotype { get; } 
        string Significance { get; }
        string SnoMedCtID { get; }
        IEnumerable<long> PubmedIds { get; }
        long LastEvaluatedDate { get; }
    }

    public enum ReviewStatus
    {
        // ReSharper disable InconsistentNaming
        no_assertion,
        no_criteria,
        single_submitter,
        multiple_submitters,
        conflicting_interpretations,
        expert_panel,
        practice_guideline
        // ReSharper restore InconsistentNaming
    }

    public enum ClinVarSignificance
    {
        //##INFO=<ID=CLNSIG,Number=.,Type=String,Description="Variant Clinical Significance, 0 - Uncertain significance, 1 - not provided, 2 - Benign, 3 - Likely benign, 4 - Likely pathogenic, 5 - Pathogenic, 6 - drug response, 7 - histocompatibility, 255 - other">
        Null,
        UncertainSignificance,
        NotProvided,
        Benign,
        LikelyBenign,
        LikelyPathogenic,
        Pathogenic,
        DrugResponse,
        Histocompatibility,
        Other
    }
}