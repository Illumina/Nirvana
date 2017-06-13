namespace VariantAnnotation.Loftee
{
	public static class LofteeFilter
	{
		public enum Filter
		{
		    // ReSharper disable InconsistentNaming
			end_trunc,		    
			incomplete_cds,
			non_can_splice_surr,
			exon_intron_undef,
			small_intron,
			non_can_splice,
			anc_allele
            // ReSharper restore InconsistentNaming
        }

        public enum Flag
		{
            // ReSharper disable InconsistentNaming
            single_exon,
			nagnag_site,
			phylocsf_weak,
			phylocsf_unlikely_orf,
			phylocsf_too_short
            // ReSharper restore InconsistentNaming
        }
    }
}