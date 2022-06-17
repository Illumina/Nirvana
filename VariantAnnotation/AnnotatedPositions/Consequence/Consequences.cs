using System;
using System.Collections.Generic;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;

namespace VariantAnnotation.AnnotatedPositions.Consequence
{
    public sealed class Consequences
    {
        private readonly VariantType _variantType;
        private readonly List<ConsequenceTag> _consequences;
        public List<ConsequenceTag> GetConsequences() => _consequences;

        private readonly IVariantEffect _variantEffect;
        private readonly IFeatureVariantEffects _featureEffect;

        private readonly (Func<bool>, ConsequenceTag)[] _tier3Consequences;

        private static readonly HashSet<ConsequenceTag> ConsequencesThatNeedTranscriptVariant = new HashSet<ConsequenceTag>
        {
            ConsequenceTag.feature_elongation, 
            ConsequenceTag.feature_truncation,
            ConsequenceTag.short_tandem_repeat_change, 
            ConsequenceTag.short_tandem_repeat_contraction,
            ConsequenceTag.short_tandem_repeat_expansion, 
            ConsequenceTag.transcript_ablation
        };

        public Consequences(VariantType variantType, IVariantEffect variantEffect, IFeatureVariantEffects featureEffect)
        {
            _variantType   = variantType;
            _consequences  = new List<ConsequenceTag>();
            _variantEffect = variantEffect;
            _featureEffect = featureEffect;

            _tier3Consequences = new List<(Func<bool>, ConsequenceTag)>
            {
                (() => _variantEffect.IsSpliceDonorVariant(),                     ConsequenceTag.splice_donor_variant),
                (() => _variantEffect.IsSpliceAcceptorVariant(),                  ConsequenceTag.splice_acceptor_variant),
                (() => _variantEffect.IsStopGained(),                             ConsequenceTag.stop_gained),
                (() => _variantEffect.IsFrameshiftVariant(),                      ConsequenceTag.frameshift_variant),
                (() => _variantEffect.IsStopLost(),                               ConsequenceTag.stop_lost),
                (() => _variantEffect.IsStartLost(),                              ConsequenceTag.start_lost),
                (() => _variantEffect.IsInframeInsertion(),                       ConsequenceTag.inframe_insertion),
                (() => _variantEffect.IsInframeDeletion(),                        ConsequenceTag.inframe_deletion),
                (() => _variantEffect.IsMissenseVariant(),                        ConsequenceTag.missense_variant),
                (() => _variantEffect.IsProteinAlteringVariant(),                 ConsequenceTag.protein_altering_variant),
                (() => _variantEffect.IsSpliceRegionVariant(),                    ConsequenceTag.splice_region_variant),
                (() => _variantEffect.IsIncompleteTerminalCodonVariant(),         ConsequenceTag.incomplete_terminal_codon_variant),
                (() => _variantEffect.IsStartRetained(),                          ConsequenceTag.start_retained_variant),
                (() => _variantEffect.IsStopRetained(),                           ConsequenceTag.stop_retained_variant),
                (() => _variantEffect.IsSynonymousVariant(),                      ConsequenceTag.synonymous_variant),
                (() => _variantEffect.IsCodingSequenceVariant(),                  ConsequenceTag.coding_sequence_variant),
                (() => _variantEffect.IsFivePrimeUtrVariant(),                    ConsequenceTag.five_prime_UTR_variant),
                (() => _variantEffect.IsThreePrimeUtrVariant(),                   ConsequenceTag.three_prime_UTR_variant),
                (() => _variantEffect.IsNonCodingTranscriptExonVariant(),         ConsequenceTag.non_coding_transcript_exon_variant),
                (() => _variantEffect.IsWithinIntron(),                           ConsequenceTag.intron_variant),
                (() => _variantEffect.IsNonsenseMediatedDecayTranscriptVariant(), ConsequenceTag.NMD_transcript_variant),
                (() => _variantEffect.IsNonCodingTranscriptVariant(),             ConsequenceTag.non_coding_transcript_variant),
                (() => _featureEffect.Elongation(),                               ConsequenceTag.feature_elongation),
                (() => _featureEffect.Truncation(),                               ConsequenceTag.feature_truncation)
            }.ToArray();
        }

        public static List<ConsequenceTag> DetermineFlankingVariantEffects(bool isDownstreamVariant) => new List<ConsequenceTag>(1)
            {isDownstreamVariant ? ConsequenceTag.downstream_gene_variant : ConsequenceTag.upstream_gene_variant};

        public void DetermineSmallVariantEffects()
        {
            GetTier1Types();
            if (_consequences.Count == 0) GetTier2Types();
            if (_consequences.Count == 0) GetTier3Types();

            if (NeedsTranscriptVariant(_variantType, _consequences)) _consequences.Add(ConsequenceTag.transcript_variant);
        }

        internal static bool NeedsTranscriptVariant(VariantType variantType, List<ConsequenceTag> consequences) => consequences.Count == 0
            ? NeedsTranscriptVariantByVariantType(variantType)
            : NeedsTranscriptVariantByConsequences(consequences);

        private static bool NeedsTranscriptVariantByConsequences(List<ConsequenceTag> consequences)
        {
            foreach (ConsequenceTag consequence in consequences)
            {
                if (!ConsequencesThatNeedTranscriptVariant.Contains(consequence)) return false;
            }

            return true;
        }

        private static bool NeedsTranscriptVariantByVariantType(VariantType variantType)
        {
            switch (variantType)
            {
                case VariantType.duplication:
                case VariantType.tandem_duplication:
                case VariantType.copy_number_variation:
                case VariantType.copy_number_loss:
                case VariantType.copy_number_gain:
                case VariantType.run_of_homozygosity:
                    return false;
                default:
                    return true;
            }
        }

        public void DetermineStructuralVariantEffect(IVariant variant)
        {
            GetTier1Types();
            if (_consequences.Count == 0) GetStructuralTier2Types();

            DetermineCopyNumberEffect(variant.Type);
            DetermineRepeatExpansionEffect(variant);

            if (NeedsTranscriptVariant(_variantType, _consequences)) _consequences.Add(ConsequenceTag.transcript_variant);
        }

        private void DetermineRepeatExpansionEffect(IVariant variant)
        {
            if (!(variant is RepeatExpansion repeatExpansion)) return;

            if (repeatExpansion.RefRepeatCount == null || repeatExpansion.RefRepeatCount == repeatExpansion.RepeatCount)
            {
                _consequences.Add(ConsequenceTag.short_tandem_repeat_change);
            }
            else if (repeatExpansion.RepeatCount > repeatExpansion.RefRepeatCount)
            {
                _consequences.Add(ConsequenceTag.short_tandem_repeat_expansion);
            }
            else
            {
                _consequences.Add(ConsequenceTag.short_tandem_repeat_contraction);
            }
        }

        private void DetermineCopyNumberEffect(VariantType variantType)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
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

            // FeatureTruncation
            if (_featureEffect.Truncation()) _consequences.Add(ConsequenceTag.feature_truncation);

            // FivePrimeDuplicatedTranscript
            if (_featureEffect.FivePrimeDuplicatedTranscript()) _consequences.Add(ConsequenceTag.five_prime_duplicated_transcript);

            // ThreePrimeDuplicatedTranscript
            if (_featureEffect.ThreePrimeDuplicatedTranscript()) _consequences.Add(ConsequenceTag.three_prime_duplicated_transcript);
        }

        private void GetTier1Types()
        {
            // TranscriptAblation
            if (_featureEffect.Ablation()) _consequences.Add(ConsequenceTag.transcript_ablation);

            // TranscriptAmplification
            if (_featureEffect.Amplification()) _consequences.Add(ConsequenceTag.transcript_amplification);
        }

        private void GetTier2Types()
        {
            // MatureMirnaVariant
            if (_variantEffect.IsMatureMirnaVariant()) _consequences.Add(ConsequenceTag.mature_miRNA_variant);
        }

        private void GetTier3Types()
        {
            foreach ((Func<bool> consequenceTest, ConsequenceTag consequenceTag) in _tier3Consequences)
            {
                if (consequenceTest()) _consequences.Add(consequenceTag);
            }
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