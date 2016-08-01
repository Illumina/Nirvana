using System.Collections.Generic;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures;

namespace CacheUtils.Archive
{
    public static class BioTypeUtilities
    {
        #region members

        private static readonly Dictionary<string, BioType> StringToBioType = new Dictionary<string, BioType>();

        private const string AmbiguousOrfBiotypeKey = "ambiguous_orf";
        private const string AntisenseBiotypeKey = "antisense";
        private const string AntisenseRnaBiotypeKey = "antisense_RNA";
        private const string BidirectionalPromoterLncRnaKey = "bidirectional_promoter_lncrna";
        private const string GuideRnaBiotypeKey = "guide_RNA";
        private const string IgCGeneBiotypeKey = "IG_C_gene";
        private const string IgCPseudoGeneBiotypeKey = "IG_C_pseudogene";
        private const string IgDGeneBiotypeKey = "IG_D_gene";
        private const string IgJGeneBiotypeKey = "IG_J_gene";
        private const string IgJPseudoGeneBiotypeKey = "IG_J_pseudogene";
        private const string IgVGeneBiotypeKey = "IG_V_gene";
        private const string IgVPseudoGeneBiotypeKey = "IG_V_pseudogene";
        private const string LongIntergenicNonCodingRnaBiotypeKey = "lincRNA";
        private const string LongNonCodingRnaBiotypeKey = "lncRNA";
        private const string MacroLongNonCodingRnaBiotypeKey = "macro_lncRNA";
        private const string MessengerRnaBiotypeKey = "mRNA";
        private const string MicroRnaBiotypeKey = "miRNA";
        private const string MiscRnaBiotypeKey = "misc_RNA";
        private const string MitochondrialRibosomalRnaBiotypeKey = "Mt_rRNA";
        private const string MitochondrialTransferRnaBiotypeKey = "Mt_tRNA";
        private const string NonCodingBiotypeKey = "non_coding";
        private const string NonsenseMediatedDecayBiotypeKey = "nonsense_mediated_decay";
        private const string NonStopDecayBiotypeKey = "non_stop_decay";
        private const string PolymorphicPseudoGene = "polymorphic_pseudogene";
        private const string ProcessedPseudoGeneBiotypeKey = "processed_pseudogene";
        private const string ProcessedTranscriptBiotypeKey = "processed_transcript";
        private const string ProteinCodingBiotypeKey = "protein_coding";
        private const string PseudoGeneBiotypeKey = "pseudogene";
        private const string RetainedIntronBiotypeKey = "retained_intron";
        private const string RetrotransposedBiotypeKey = "retrotransposed";
        private const string RibonucleaseMrpBiotypeKey = "RNase_MRP_RNA";
        private const string RibonucleasePBiotypeKey = "RNase_P_RNA";
        private const string RibosomalRnaBiotypeKey = "rRNA";
        private const string RibozymeBiotypeKey = "ribozyme";
        private const string SenseIntronicBiotypeKey = "sense_intronic";
        private const string SenseOverlappingBiotypeKey = "sense_overlapping";
        private const string SmallRnaBiotypeKey = "sRNA";
        private const string SmallCytoplasmicRnaBiotypeKey = "scRNA";
        private const string SmallCajalBodySpecificRnaBiotypeKey = "scaRNA";
        private const string SmallNuclearRnaBiotypeKey = "snRNA";
        private const string SmallNucleolarRnaBiotypeKey = "snoRNA";
        private const string SignalRecognitionParticleRnaBiotypeKey = "SRP_RNA";
        private const string TelomeraseRnaBiotypeKey = "telomerase_RNA";
        private const string ThreePrimeOverlappingNcRnaBiotypeKey = "3prime_overlapping_ncrna";
        private const string TranscribedProcessedPseudoGeneBiotypeKey = "transcribed_processed_pseudogene";
        private const string TranscribedUnitaryPseudoGeneBiotypeKey = "transcribed_unitary_pseudogene";
        private const string TranscribedUnprocessedPseudoGeneBiotypeKey = "transcribed_unprocessed_pseudogene";
        private const string TranscriptionElongationComplexBiotypeKey = "TEC";
        private const string TranslatedProcessedPseudogeneKey = "translated_processed_pseudogene";
        private const string TranslatedUnprocessedPseudogeneKey = "translated_unprocessed_pseudogene";
        private const string TransferRnaBiotypeKey = "tRNA";
        private const string TrCGeneBiotypeKey = "TR_C_gene";
        private const string TrDGeneBiotypeKey = "TR_D_gene";
        private const string TrJGeneBiotypeKey = "TR_J_gene";
        private const string TrJPseudoGeneBiotypeKey = "TR_J_pseudogene";
        private const string TrVGeneBiotypeKey = "TR_V_gene";
        private const string TrVPseudoGeneBiotypeKey = "TR_V_pseudogene";
        private const string UnitaryPseudoGeneBiotypeKey = "unitary_pseudogene";
        private const string UnprocessedPseudoGeneBiotypeKey = "unprocessed_pseudogene";
        private const string VaultRnaBiotypeKey = "vaultRNA";
        private const string YRnaBiotypeKey = "Y_RNA";

        #endregion

        // constructor
        static BioTypeUtilities()
        {
            AddBioType(AmbiguousOrfBiotypeKey, BioType.AmbiguousOrf);
            AddBioType(AntisenseBiotypeKey, BioType.Antisense);
            AddBioType(AntisenseRnaBiotypeKey, BioType.AntisenseRNA);
            AddBioType(BidirectionalPromoterLncRnaKey, BioType.BidirectionalPromoterLncRNA);
            AddBioType(GuideRnaBiotypeKey, BioType.GuideRNA);
            AddBioType(IgCGeneBiotypeKey, BioType.IgCGene);
            AddBioType(IgCPseudoGeneBiotypeKey, BioType.IgCPseudoGene);
            AddBioType(IgDGeneBiotypeKey, BioType.IgDGene);
            AddBioType(IgJGeneBiotypeKey, BioType.IgJGene);
            AddBioType(IgJPseudoGeneBiotypeKey, BioType.IgJPseudoGene);
            AddBioType(IgVGeneBiotypeKey, BioType.IgVGene);
            AddBioType(IgVPseudoGeneBiotypeKey, BioType.IgVPseudoGene);
            AddBioType(LongIntergenicNonCodingRnaBiotypeKey, BioType.LongIntergenicNonCodingRna);
            AddBioType(LongNonCodingRnaBiotypeKey, BioType.lncRNA);
            AddBioType(MacroLongNonCodingRnaBiotypeKey, BioType.macroLncRNA);
            AddBioType(MessengerRnaBiotypeKey, BioType.mRNA);
            AddBioType(MicroRnaBiotypeKey, BioType.miRNA);
            AddBioType(MiscRnaBiotypeKey, BioType.RNA);
            AddBioType(MitochondrialRibosomalRnaBiotypeKey, BioType.MitochondrialRibosomalRna);
            AddBioType(MitochondrialTransferRnaBiotypeKey, BioType.MitochondrialTransferRna);
            AddBioType(NonCodingBiotypeKey, BioType.NonCoding);
            AddBioType(NonsenseMediatedDecayBiotypeKey, BioType.NonsenseMediatedDecay);
            AddBioType(NonStopDecayBiotypeKey, BioType.NonStopDecay);
            AddBioType(PolymorphicPseudoGene, BioType.PolymorphicPseudoGene);
            AddBioType(ProcessedPseudoGeneBiotypeKey, BioType.ProcessedPseudoGene);
            AddBioType(ProcessedTranscriptBiotypeKey, BioType.ProcessedTranscript);
            AddBioType(ProteinCodingBiotypeKey, BioType.ProteinCoding);
            AddBioType(PseudoGeneBiotypeKey, BioType.PseudoGene);
            AddBioType(RetainedIntronBiotypeKey, BioType.RetainedIntron);
            AddBioType(RetrotransposedBiotypeKey, BioType.Retrotransposed);
            AddBioType(RibonucleaseMrpBiotypeKey, BioType.RibonucleaseMrpRna);
            AddBioType(RibonucleasePBiotypeKey, BioType.RibonucleasePRna);
            AddBioType(RibosomalRnaBiotypeKey, BioType.RibosomalRna);
            AddBioType(RibozymeBiotypeKey, BioType.Ribozyme);
            AddBioType(SenseIntronicBiotypeKey, BioType.SenseIntronic);
            AddBioType(SenseOverlappingBiotypeKey, BioType.SenseOverlapping);
            AddBioType(SignalRecognitionParticleRnaBiotypeKey, BioType.SignalRecognitionParticleRNA);
            AddBioType(SmallRnaBiotypeKey, BioType.sRNA);
            AddBioType(SmallCytoplasmicRnaBiotypeKey, BioType.scRNA);
            AddBioType(SmallCajalBodySpecificRnaBiotypeKey, BioType.scaRNA);
            AddBioType(SmallNuclearRnaBiotypeKey, BioType.snRNA);
            AddBioType(SmallNucleolarRnaBiotypeKey, BioType.snoRNA);
            AddBioType(TelomeraseRnaBiotypeKey, BioType.TelomeraseRNA);
            AddBioType(ThreePrimeOverlappingNcRnaBiotypeKey, BioType.ThreePrimeOverlappingNcRna);
            AddBioType(TranscribedProcessedPseudoGeneBiotypeKey, BioType.TranscribedProcessedPseudoGene);
            AddBioType(TranscribedUnitaryPseudoGeneBiotypeKey, BioType.TranscribedUnitaryPseudoGene);
            AddBioType(TranscribedUnprocessedPseudoGeneBiotypeKey, BioType.TranscribedUnprocessedPseudoGene);
            AddBioType(TranscriptionElongationComplexBiotypeKey, BioType.TranscriptionElongationComplex);
            AddBioType(TransferRnaBiotypeKey, BioType.tRNA);
            AddBioType(TranslatedProcessedPseudogeneKey, BioType.TranslatedProcessedPseudogene);
            AddBioType(TranslatedUnprocessedPseudogeneKey, BioType.TranslatedUnprocessedPseudogene);
            AddBioType(TrCGeneBiotypeKey, BioType.TrCGene);
            AddBioType(TrDGeneBiotypeKey, BioType.TrDGene);
            AddBioType(TrJGeneBiotypeKey, BioType.TrJGene);
            AddBioType(TrJPseudoGeneBiotypeKey, BioType.TrJPseudoGene);
            AddBioType(TrVGeneBiotypeKey, BioType.TrVGene);
            AddBioType(TrVPseudoGeneBiotypeKey, BioType.TrVPseudoGene);
            AddBioType(UnitaryPseudoGeneBiotypeKey, BioType.UnitaryPseudoGene);
            AddBioType(UnprocessedPseudoGeneBiotypeKey, BioType.UnprocessedPseudoGene);
            AddBioType(VaultRnaBiotypeKey, BioType.VaultRNA);
            AddBioType(YRnaBiotypeKey, BioType.YRNA);
        }

        /// <summary>
        /// adds the biotype to both dictionaries
        /// </summary>
        private static void AddBioType(string s, BioType bioType)
        {
            StringToBioType[s] = bioType;
        }

        /// <summary>
        /// returns the biotype given the string representation
        /// </summary>
        public static BioType GetBiotypeFromString(string s)
        {
            BioType ret;
            if (!StringToBioType.TryGetValue(s, out ret))
            {
                throw new GeneralException($"Unable to find the specified BioType ({s}) in the BioType dictionary.");
            }

            return ret;
        }
    }

}
