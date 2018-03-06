namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public enum ConsequenceTag : byte
    {
        // ReSharper disable InconsistentNaming
        coding_sequence_variant,
        copy_number_increase,
        copy_number_decrease,
        copy_number_change,
        downstream_gene_variant,
        feature_elongation,
        five_prime_UTR_variant,
        frameshift_variant,
        incomplete_terminal_codon_variant,
        inframe_deletion,
        inframe_insertion,
        start_lost,
        start_retained_variant,
        intron_variant,
        missense_variant,
        mature_miRNA_variant,
        non_coding_transcript_exon_variant,
        non_coding_transcript_variant,
        NMD_transcript_variant,
        protein_altering_variant,
        regulatory_region_variant,
        regulatory_region_ablation,
        regulatory_region_amplification,
        splice_acceptor_variant,
        splice_donor_variant,
        splice_region_variant,
        stop_gained,
        stop_lost,
        stop_retained_variant,
        synonymous_variant,
        three_prime_UTR_variant,
        transcript_amplification,
        transcript_ablation,
        transcript_truncation,
        upstream_gene_variant,
        short_tandem_repeat_change,
        short_tandem_repeat_expansion,
        short_tandem_repeat_contraction,
        transcript_variant,
        unidirectional_gene_fusion
        // ReSharper restore InconsistentNaming
    }

    public static class ConsequenceUtil
    {
        public static string GetConsequence(ConsequenceTag consequence)
        {
            if (consequence == ConsequenceTag.five_prime_UTR_variant) return "5_prime_UTR_variant";
            return consequence == ConsequenceTag.three_prime_UTR_variant ? "3_prime_UTR_variant" : consequence.ToString();
        }
    }
}