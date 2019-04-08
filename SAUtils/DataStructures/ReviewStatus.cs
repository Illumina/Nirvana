namespace SAUtils.DataStructures
{
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
		practice_guideline
		// ReSharper restore InconsistentNaming
	}
}
