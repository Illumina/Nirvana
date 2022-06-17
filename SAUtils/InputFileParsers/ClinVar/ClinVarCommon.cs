using System.Collections.Generic;
using System.Linq;

namespace SAUtils.InputFileParsers.ClinVar
{
    public static class ClinVarCommon
    {
        public static string NormalizeAllele(string allele)
        {
            if (string.IsNullOrEmpty(allele)) return "-";
            return allele == "N" ? null : allele;
        }
        
        public static readonly HashSet<string> ValidPathogenicity = new HashSet<string>
        {
            "uncertain significance",
            "not provided",
            "benign",
            "likely benign",
            "likely pathogenic",
            "pathogenic",
            "drug response",
            "histocompatibility",
            "association",
            "risk factor",
            "protective",
            "affects",
            "conflicting data from submitters",
            "other",
            "association not found",
            "confers sensitivity",
            "no interpretation for the single variant",// observed in VCV XML only
            
            "conflicting interpretations of pathogenicity", // observed in VCV XML only
            "established risk allele", // observed in VCV XML only
            "likely risk allele"                            // observed in VCV XML only
        };
        public enum ReviewStatus
        {
            // ReSharper disable InconsistentNaming
            no_assertion,
            no_criteria,
            single_submitter,
            multiple_submitters,
            multiple_submitters_no_conflict,
            conflicting_interpretations,
            expert_panel,
            practice_guideline,
            no_interpretation_single
            // ReSharper restore InconsistentNaming
        }
        public static readonly Dictionary<string, ReviewStatus> ReviewStatusNameMapping = new Dictionary<string, ReviewStatus>
        {
            ["no_assertion"] = ReviewStatus.no_assertion,
            ["no_criteria"]  = ReviewStatus.no_criteria,
            ["guideline"]    = ReviewStatus.practice_guideline,
            ["single"]       = ReviewStatus.single_submitter,
            ["mult"]         = ReviewStatus.multiple_submitters,
            ["conf"]         = ReviewStatus.conflicting_interpretations,
            ["exp"]          = ReviewStatus.expert_panel,
            // the following are the long forms found in XML
            ["no assertion provided"]                                = ReviewStatus.no_assertion,
            ["no assertion criteria provided"]                       = ReviewStatus.no_criteria,
            ["practice guideline"]                                   = ReviewStatus.practice_guideline,
            ["criteria provided, conflicting interpretations"]       = ReviewStatus.conflicting_interpretations,
            ["reviewed by expert panel"]                             = ReviewStatus.expert_panel,
            ["classified by multiple submitters"]                    = ReviewStatus.multiple_submitters,
            ["criteria provided, multiple submitters, no conflicts"] = ReviewStatus.multiple_submitters_no_conflict,
            ["criteria provided, single submitter"]                  = ReviewStatus.single_submitter,
            ["no interpretation for the single variant"]  = ReviewStatus.no_interpretation_single
        };

        public static readonly Dictionary<ReviewStatus, string> ReviewStatusStrings = new Dictionary<ReviewStatus, string>
        {
            [ReviewStatus.no_criteria]                     = "no assertion criteria provided",
            [ReviewStatus.no_assertion]                    = "no assertion provided",
            [ReviewStatus.expert_panel]                    = "reviewed by expert panel",
            [ReviewStatus.single_submitter]                = "criteria provided, single submitter",
            [ReviewStatus.practice_guideline]              = "practice guideline",
            [ReviewStatus.multiple_submitters]             = "classified by multiple submitters",
            [ReviewStatus.conflicting_interpretations]     = "criteria provided, conflicting interpretations",
            [ReviewStatus.multiple_submitters_no_conflict] = "criteria provided, multiple submitters, no conflicts",
            [ReviewStatus.no_interpretation_single]        = "no interpretation for the single variant"
        };
        
        public static string[] GetSignificances(string description, string explanation)
        {
            if(string.IsNullOrEmpty(explanation)) return description?.ToLower().Split('/', ',', ';').Select(x=>x.Trim()).ToArray();
            //<Explanation DataSource="ClinVar" Type="public">Pathogenic(1);Uncertain significance(1)</Explanation>
            var significances =new List<string>();
            foreach (var significance in explanation.ToLower().Split('/',';'))
            {
                var openParenthesisIndex = significance.IndexOf('(');
                significances.Add(openParenthesisIndex < 0 ? significance.Trim() : significance.Substring(0, openParenthesisIndex).Trim());
            }

            return significances.ToArray();
        }

    }
}