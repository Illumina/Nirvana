using System;
using System.Collections.Generic;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.Helpers
{
    public static class BioTypeHelper
    {
        private static readonly Dictionary<string, BioType> StringToBioTypes;

        static BioTypeHelper()
        {
            StringToBioTypes = new Dictionary<string, BioType>
            {
                ["aligned_transcript"]                 = BioType.aligned_transcript,
                ["ambiguous_orf"]                      = BioType.ambiguous_orf,
                ["antisense"]                          = BioType.antisense,
                ["antisense_RNA"]                      = BioType.antisense_RNA,
                ["bidirectional_promoter_lncRNA"]      = BioType.bidirectional_promoter_lncRNA,
                ["guide_RNA"]                          = BioType.guide_RNA,
                ["IG_pseudogene"]                      = BioType.IG_pseudogene,
                ["IG_C_gene"]                          = BioType.IG_C_gene,
                ["IG_C_pseudogene"]                    = BioType.IG_C_pseudogene,
                ["IG_D_gene"]                          = BioType.IG_D_gene,
                ["IG_J_gene"]                          = BioType.IG_J_gene,
                ["IG_J_pseudogene"]                    = BioType.IG_J_pseudogene,
                ["IG_V_gene"]                          = BioType.IG_V_gene,
                ["IG_V_pseudogene"]                    = BioType.IG_V_pseudogene,
                ["lincRNA"]                            = BioType.lincRNA,
                ["lncRNA"]                             = BioType.lncRNA,
                ["macro_lncRNA"]                       = BioType.macro_lncRNA,
                ["mRNA"]                               = BioType.mRNA,
                ["miRNA"]                              = BioType.miRNA,
                ["misc_RNA"]                           = BioType.misc_RNA,
                ["Mt_rRNA"]                            = BioType.Mt_rRNA,
                ["Mt_tRNA"]                            = BioType.Mt_tRNA,
                ["non_coding"]                         = BioType.non_coding,
                ["nonsense_mediated_decay"]            = BioType.nonsense_mediated_decay,
                ["non_stop_decay"]                     = BioType.non_stop_decay,
                ["other"]                              = BioType.other,
                ["polymorphic_pseudogene"]             = BioType.polymorphic_pseudogene,
                ["processed_pseudogene"]               = BioType.processed_pseudogene,
                ["processed_transcript"]               = BioType.processed_transcript,
                ["protein_coding"]                     = BioType.protein_coding,
                ["pseudogene"]                         = BioType.pseudogene,
                ["retained_intron"]                    = BioType.retained_intron,
                ["retrotransposed"]                    = BioType.retrotransposed,
                ["RNase_MRP_RNA"]                      = BioType.RNase_MRP_RNA,
                ["RNase_P_RNA"]                        = BioType.RNase_P_RNA,
                ["rRNA"]                               = BioType.rRNA,
                ["ribozyme"]                           = BioType.ribozyme,
                ["sense_intronic"]                     = BioType.sense_intronic,
                ["sense_overlapping"]                  = BioType.sense_overlapping,
                ["SRP_RNA"]                            = BioType.SRP_RNA,
                ["sRNA"]                               = BioType.sRNA,
                ["scRNA"]                              = BioType.scRNA,
                ["scaRNA"]                             = BioType.scaRNA,
                ["snRNA"]                              = BioType.snRNA,
                ["snoRNA"]                             = BioType.snoRNA,
                ["telomerase_RNA"]                     = BioType.telomerase_RNA,
                ["3prime_overlapping_ncrna"]           = BioType.three_prime_overlapping_ncRNA,
                ["3prime_overlapping_ncRNA"]           = BioType.three_prime_overlapping_ncRNA,
                ["transcribed_processed_pseudogene"]   = BioType.transcribed_processed_pseudogene,
                ["translated_unprocessed_pseudogene"]  = BioType.translated_unprocessed_pseudogene,
                ["transcribed_unitary_pseudogene"]     = BioType.transcribed_unitary_pseudogene,
                ["TEC"]                                = BioType.TEC,
                ["tRNA"]                               = BioType.tRNA,
                ["translated_processed_pseudogene"]    = BioType.translated_processed_pseudogene,
                ["transcribed_unprocessed_pseudogene"] = BioType.transcribed_unprocessed_pseudogene,
                ["TR_C_gene"]                          = BioType.TR_C_gene,
                ["TR_D_gene"]                          = BioType.TR_D_gene,
                ["TR_J_gene"]                          = BioType.TR_J_gene,
                ["TR_J_pseudogene"]                    = BioType.TR_J_pseudogene,
                ["TR_V_gene"]                          = BioType.TR_V_gene,
                ["TR_V_pseudogene"]                    = BioType.TR_V_pseudogene,
                ["unitary_pseudogene"]                 = BioType.unitary_pseudogene,
                ["unprocessed_pseudogene"]             = BioType.unprocessed_pseudogene,
                ["vaultRNA"]                           = BioType.vaultRNA,
                ["Y_RNA"]                              = BioType.Y_RNA
            };
        }

        public static BioType GetBioType(string s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (!StringToBioTypes.TryGetValue(s, out var ret)) throw new InvalidOperationException($"The specified biotype ({s}) was not found in the BioType enum.");
            return ret;
        }
    }
}
