﻿namespace Variants
{
    public enum VariantType
    {
        // ReSharper disable InconsistentNaming
        unknown = 0,

        // small variants
        SNV       = 2,
        insertion = 3,
        deletion  = 4,
        indel     = 5,
        MNV       = 6,

        // structural variants
        duplication                     = 10,
        complex_structural_alteration   = 11,
        structural_alteration           = 12,
        tandem_duplication              = 13,
        translocation_breakend          = 14,
        inversion                       = 15,
        mobile_element_insertion        = 16,
        mobile_element_deletion         = 17,
        novel_sequence_insertion        = 18,
        short_tandem_repeat_variation   = 19,

        // CNVs
        copy_number_variation = 30,
        copy_number_loss      = 31,
        copy_number_gain      = 32,

        // non variants
        run_of_homozygosity = 33,
        
        // misc
        reference = 42,
        non_informative_allele = 43
    }
    // ReSharper restore InconsistentNaming
}
