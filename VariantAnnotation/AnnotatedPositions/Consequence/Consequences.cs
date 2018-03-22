using System.Collections.Generic;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Positions;

namespace VariantAnnotation.AnnotatedPositions.Consequence
{
    public sealed class Consequences
    {
        private readonly List<ConsequenceTag> _consequences;
        public List<ConsequenceTag> GetConsequences() => _consequences;

        private readonly IVariantEffect _variantEffect;

        private readonly IFeatureVariantEffects _featureEffect;


        public Consequences(IVariantEffect variantEffect = null, IFeatureVariantEffects featureEffect = null)
        {
            _consequences = new List<ConsequenceTag>();
            _variantEffect = variantEffect;
            _featureEffect = featureEffect;
        }

        /// <summary>
        /// determines the flanking variant's functional consequence
        /// </summary>
        public void DetermineFlankingVariantEffects(bool isDownstreamVariant)
        {
            _consequences.Add(isDownstreamVariant
                ? ConsequenceTag.downstream_gene_variant
                : ConsequenceTag.upstream_gene_variant);
        }


        /// <summary>
        /// determines the variant's functional consequence
        /// </summary>
        public void DetermineSmallVariantEffects()
        {
            GetTier1Types();
            if (_consequences.Count == 0) GetTier2Types();
            if (_consequences.Count == 0) GetTier3Types();
            if (_consequences.Count == 0) _consequences.Add(ConsequenceTag.transcript_variant);
        }

        public void DetermineStructuralVariantEffect(VariantType variantType, bool addGeneFusion)
        {
            GetTier1Types();
            if (_consequences.Count == 0) GetStructuralTier2Types();
            if (addGeneFusion) _consequences.Add(ConsequenceTag.unidirectional_gene_fusion);

            DetermineCopyNumberEffect(variantType);
            DetermineRepeatExpansionEffect(variantType);
            if (_consequences.Count == 0) _consequences.Add(ConsequenceTag.transcript_variant);
        }

        private void DetermineRepeatExpansionEffect(VariantType variantType)
        {
            switch (variantType)
            {
                case VariantType.short_tandem_repeat_variation:
                    _consequences.Add(ConsequenceTag.short_tandem_repeat_change);
                    break;
                case VariantType.short_tandem_repeat_contraction:
                    _consequences.Add(ConsequenceTag.short_tandem_repeat_contraction);
                    break;
                case VariantType.short_tandem_repeat_expansion:
                    _consequences.Add(ConsequenceTag.short_tandem_repeat_expansion);
                    break;
            }
        }

        private void DetermineCopyNumberEffect(VariantType variantType)
        {
            switch (variantType)
            {
                case VariantType.copy_number_gain:
                    _consequences.Add(ConsequenceTag.copy_number_increase);
                    break;
                case VariantType.copy_number_loss:
                    _consequences.Add(ConsequenceTag.copy_number_decrease);
                    break;
                case VariantType.copy_number_variation:
                    _consequences.Add(ConsequenceTag.copy_number_change);
                    break;
            }
        }

        private void GetStructuralTier2Types()
        {
            // FeatureElongation
            if (_featureEffect.Elongation()) _consequences.Add(ConsequenceTag.feature_elongation);

            // TranscriptTruncation
            if (_featureEffect.Truncation()) _consequences.Add(ConsequenceTag.transcript_truncation);
        }

        /// <summary>
        /// populates the consequences list with tier 1 consequences if found (NOTE: Tests are done in rank order)
        /// </summary>
        private void GetTier1Types()
        {
            // TranscriptAblation
            if (_featureEffect.Ablation()) _consequences.Add(ConsequenceTag.transcript_ablation);

            // TranscriptAmplification
            if (_featureEffect.Amplification()) _consequences.Add(ConsequenceTag.transcript_amplification);

        }

        /// <summary>
        /// populates the consequences list with tier 2 consequences if found (NOTE: Tests are done in rank order)
        /// </summary>
        private void GetTier2Types()
        {
            // MatureMirnaVariant
            if (_variantEffect.IsMatureMirnaVariant()) _consequences.Add(ConsequenceTag.mature_miRNA_variant);
        }


        /// <summary>
        /// populates the consequences list with tier 3 consequences if found (NOTE: Tests are done in rank order)
        /// </summary>
        private void GetTier3Types()
        {
            // SpliceDonorVariant
            if (_variantEffect.IsSpliceDonorVariant()) _consequences.Add(ConsequenceTag.splice_donor_variant);

            // SpliceAcceptorVariant
            if (_variantEffect.IsSpliceAcceptorVariant()) _consequences.Add(ConsequenceTag.splice_acceptor_variant);

            // StopGained
            if (_variantEffect.IsStopGained()) _consequences.Add(ConsequenceTag.stop_gained);

            // FrameshiftVariant
            if (_variantEffect.IsFrameshiftVariant()) _consequences.Add(ConsequenceTag.frameshift_variant);

            // StopLost
            if (_variantEffect.IsStopLost()) _consequences.Add(ConsequenceTag.stop_lost);
            if (_variantEffect.IsStartLost()) _consequences.Add(ConsequenceTag.start_lost);

            // InframeInsertion
            if (_variantEffect.IsInframeInsertion()) _consequences.Add(ConsequenceTag.inframe_insertion);

            // InframeDeletion
            if (_variantEffect.IsInframeDeletion()) _consequences.Add(ConsequenceTag.inframe_deletion);

            // MissenseVariant
            if (_variantEffect.IsMissenseVariant()) _consequences.Add(ConsequenceTag.missense_variant);

            // ProteinAlteringVariant
            if (_variantEffect.IsProteinAlteringVariant()) _consequences.Add(ConsequenceTag.protein_altering_variant);

            // SpliceRegionVariant
            if (_variantEffect.IsSpliceRegionVariant()) _consequences.Add(ConsequenceTag.splice_region_variant);

            // IncompleteTerminalCodonVariant
            if (_variantEffect.IsIncompleteTerminalCodonVariant()) _consequences.Add(ConsequenceTag.incomplete_terminal_codon_variant);

            // StartRetainedVariant
            if (_variantEffect.IsStartRetained()) _consequences.Add(ConsequenceTag.start_retained_variant);

            // StopRetainedVariant
            if (_variantEffect.IsStopRetained()) _consequences.Add(ConsequenceTag.stop_retained_variant);

            // SynonymousVariant
            if (_variantEffect.IsSynonymousVariant()) _consequences.Add(ConsequenceTag.synonymous_variant);

            // CodingSequenceVariant
            if (_variantEffect.IsCodingSequenceVariant()) _consequences.Add(ConsequenceTag.coding_sequence_variant);

            // FivePrimeUtrVariant
            if (_variantEffect.IsFivePrimeUtrVariant()) _consequences.Add(ConsequenceTag.five_prime_UTR_variant);

            // ThreePrimeUtrVariant
            if (_variantEffect.IsThreePrimeUtrVariant()) _consequences.Add(ConsequenceTag.three_prime_UTR_variant);

            // NonCodingTranscriptExonVariant
            if (_variantEffect.IsNonCodingTranscriptExonVariant()) _consequences.Add(ConsequenceTag.non_coding_transcript_exon_variant);

            // IntronVariant
            if (_variantEffect.IsWithinIntron()) _consequences.Add(ConsequenceTag.intron_variant);

            // NonsenseMediatedDecayTranscriptVariant
            if (_variantEffect.IsNonsenseMediatedDecayTranscriptVariant()) _consequences.Add(ConsequenceTag.NMD_transcript_variant);

            // NonCodingTranscriptVariant
            if (_variantEffect.IsNonCodingTranscriptVariant()) _consequences.Add(ConsequenceTag.non_coding_transcript_variant);

            // FeatureElongation
            if (_featureEffect.Elongation()) _consequences.Add(ConsequenceTag.feature_elongation);

            // TranscriptTruncation
            if (_featureEffect.Truncation()) _consequences.Add(ConsequenceTag.transcript_truncation);
        }

        public void DetermineRegulatoryVariantEffects()
        {
            // RegulatoryRegionAmplification
            if (_featureEffect.Amplification()) _consequences.Add(ConsequenceTag.regulatory_region_amplification);

            // RegulatoryRegionAblation
            if (_featureEffect.Ablation()) _consequences.Add(ConsequenceTag.regulatory_region_ablation);

            // RegulatoryRegionVariant
            _consequences.Add(ConsequenceTag.regulatory_region_variant);
        }
    }


}