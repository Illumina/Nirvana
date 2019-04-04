using System.Collections.Generic;
using System.Collections.Immutable;

namespace SAUtils.DataStructures
{
    public static class ReviewStatusMapping
    {
        private const string DefaultReviewStatus = "no assertion provided";

        private static readonly ImmutableDictionary<string, ReviewStatus> StringToReviewStatus = new Dictionary<string, ReviewStatus>
        {
            ["no_assertion"] = ReviewStatus.no_assertion,
            ["no_criteria"] = ReviewStatus.no_criteria,
            ["guideline"] = ReviewStatus.practice_guideline,
            ["single"] = ReviewStatus.single_submitter,
            ["mult"] = ReviewStatus.multiple_submitters,
            ["conf"] = ReviewStatus.conflicting_interpretations,
            ["exp"] = ReviewStatus.expert_panel,
            // the following are the long forms found in XML
            ["no assertion provided"] = ReviewStatus.no_assertion,
            ["no assertion criteria provided"] = ReviewStatus.no_criteria,
            ["practice guideline"] = ReviewStatus.practice_guideline,
            ["criteria provided, conflicting interpretations"] = ReviewStatus.conflicting_interpretations,
            ["reviewed by expert panel"] = ReviewStatus.expert_panel,
            ["classified by multiple submitters"] = ReviewStatus.multiple_submitters,
            ["criteria provided, multiple submitters, no conflicts"] = ReviewStatus.multiple_submitters_no_conflict,
            ["criteria provided, single submitter"] = ReviewStatus.single_submitter
        }.ToImmutableDictionary();

        private static readonly Dictionary<ReviewStatus, string> ReviewStatusToString = new Dictionary<ReviewStatus, string>
        {
            [ReviewStatus.no_criteria] = "no assertion criteria provided",
            [ReviewStatus.no_assertion] = "no assertion provided",
            [ReviewStatus.expert_panel] = "reviewed by expert panel",
            [ReviewStatus.single_submitter] = "criteria provided, single submitter",
            [ReviewStatus.practice_guideline] = "practice guideline",
            [ReviewStatus.multiple_submitters] = "classified by multiple submitters",
            [ReviewStatus.conflicting_interpretations] = "criteria provided, conflicting interpretations",
            [ReviewStatus.multiple_submitters_no_conflict] = "criteria provided, multiple submitters, no conflicts"
        };

        public static string FormatReviewStatus(string input) => 
            StringToReviewStatus.TryGetValue(input, out var statusEnum) 
            ? ReviewStatusToString[statusEnum]
            : DefaultReviewStatus;
    }
}