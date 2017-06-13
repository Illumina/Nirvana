using System.Collections.Generic;
using VariantAnnotation.DataStructures.Intervals;
using VariantAnnotation.Interface;

namespace VariantAnnotation.Algorithms.Consequences
{
    public sealed class Consequences
    {
        #region members

        private readonly VariantEffect _variantEffect;
        private readonly List<ConsequenceType> _consequences;

        private readonly Dictionary<ConsequenceType, string> _consequenceDescriptors;

        #endregion

        #region Sequence Ontology strings

        private const string CodingSequenceVariantKey = "coding_sequence_variant";
        private const string CopyNumberIncreaseKey = "copy_number_increase";
        private const string CopyNumberDecreaseKey = "copy_number_decrease";
        private const string CopyNumberChangeKey = "copy_number_change";
        private const string DownstreamGeneVariantKey = "downstream_gene_variant";
        private const string FeatureElongationKey = "feature_elongation";
        private const string FeatureTruncationKey = "feature_truncation";
        private const string FivePrimeUtrVariantKey = "5_prime_UTR_variant";
        private const string FrameshiftVariantKey = "frameshift_variant";
        private const string IncompleteTerminalCodonVariantKey = "incomplete_terminal_codon_variant";
        private const string InframeDeletionKey = "inframe_deletion";
        private const string InframeInsertionKey = "inframe_insertion";
        private const string StartLostKey = "start_lost";
        private const string IntronVariantKey = "intron_variant";
        private const string MissenseVariantKey = "missense_variant";
        private const string MatureMirnaVariantKey = "mature_miRNA_variant";
        private const string NonCodingExonVariantKey = "non_coding_transcript_exon_variant";
        private const string NonCodingTranscriptVariantKey = "non_coding_transcript_variant";
        private const string NonsenseMediatedDecayTranscriptVariantKey = "NMD_transcript_variant";
        private const string ProteinAlteringVariantKey = "protein_altering_variant";
        internal const string RegulatoryRegionVariantKey = "regulatory_region_variant";
        private const string RegulatoryRegionAblationVariantKey = "regulatory_region_ablation";
        private const string RegulatoryRegionAmplificationVariantKey = "regulatory_region_amplification";
        private const string SpliceAcceptorVariantKey = "splice_acceptor_variant";
        private const string SpliceDonorVariantKey = "splice_donor_variant";
        private const string SpliceRegionVariantKey = "splice_region_variant";
        private const string StopGainedKey = "stop_gained";
        private const string StopLostKey = "stop_lost";
        private const string StopRetainedVariantKey = "stop_retained_variant";
        private const string SynonymousVariantKey = "synonymous_variant";
        private const string ThreePrimeUtrVariantKey = "3_prime_UTR_variant";
        private const string TranscriptAmplificationKey = "transcript_amplification";
        private const string TranscriptAblatioinKey = "transcript_ablation";
        private const string TranscriptTruncationKey = "transcript_truncation";
        private const string UnknownKey = "unknown";
        private const string UpstreamGeneVariantKey = "upstream_gene_variant";
        private const string ShortTandemRepeatChangeKey = "short_tandem_repeat_change";
        private const string ShortTandemRepeatExpansionKey = "short_tandem_repeat_expansion";
        private const string ShortTandemRepeatContractionKey = "short_tandem_repeat_contraction";
        private const string TranscriptVariantKey = "transcript_variant";
        private const string GeneFusionKey = "unidirectional_gene_fusion";


        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public Consequences(VariantEffect variantEffect)
        {
            _variantEffect = variantEffect;
            _consequences = new List<ConsequenceType>();

            _consequenceDescriptors = new Dictionary<ConsequenceType, string>
            {
                [ConsequenceType.CodingSequenceVariant] = CodingSequenceVariantKey,
                [ConsequenceType.CopyNumberIncrease] = CopyNumberIncreaseKey,
                [ConsequenceType.CopyNumberDecrease] = CopyNumberDecreaseKey,
                [ConsequenceType.CopyNumberChange] = CopyNumberChangeKey,
                [ConsequenceType.DownstreamGeneVariant] = DownstreamGeneVariantKey,
                [ConsequenceType.FeatureElongation] = FeatureElongationKey,
                [ConsequenceType.FeatureTruncation] = FeatureTruncationKey,
                [ConsequenceType.FivePrimeUtrVariant] = FivePrimeUtrVariantKey,
                [ConsequenceType.FrameshiftVariant] = FrameshiftVariantKey,
                [ConsequenceType.IncompleteTerminalCodonVariant] = IncompleteTerminalCodonVariantKey,
                [ConsequenceType.InframeDeletion] = InframeDeletionKey,
                [ConsequenceType.InframeInsertion] = InframeInsertionKey,
                [ConsequenceType.StartLost] = StartLostKey,
                [ConsequenceType.IntronVariant] = IntronVariantKey,
                [ConsequenceType.MissenseVariant] = MissenseVariantKey,
                [ConsequenceType.MatureMirnaVariant] = MatureMirnaVariantKey,
                [ConsequenceType.NonCodingTranscriptExonVariant] = NonCodingExonVariantKey,
                [ConsequenceType.NonCodingTranscriptVariant] = NonCodingTranscriptVariantKey,
                [ConsequenceType.NonsenseMediatedDecayTranscriptVariant] = NonsenseMediatedDecayTranscriptVariantKey,
                [ConsequenceType.ProteinAlteringVariant] = ProteinAlteringVariantKey,
                [ConsequenceType.RegulatoryRegionAblation] = RegulatoryRegionAblationVariantKey,
                [ConsequenceType.RegulatoryRegionAmplification] = RegulatoryRegionAmplificationVariantKey,
                [ConsequenceType.RegulatoryRegionVariant] = RegulatoryRegionVariantKey,
                [ConsequenceType.SpliceAcceptorVariant] = SpliceAcceptorVariantKey,
                [ConsequenceType.SpliceDonorVariant] = SpliceDonorVariantKey,
                [ConsequenceType.SpliceRegionVariant] = SpliceRegionVariantKey,
                [ConsequenceType.StopGained] = StopGainedKey,
                [ConsequenceType.StopLost] = StopLostKey,
                [ConsequenceType.StopRetainedVariant] = StopRetainedVariantKey,
                [ConsequenceType.SynonymousVariant] = SynonymousVariantKey,
                [ConsequenceType.ThreePrimeUtrVariant] = ThreePrimeUtrVariantKey,
                [ConsequenceType.TranscriptAmplification] = TranscriptAmplificationKey,
                [ConsequenceType.TranscriptTruncation] = TranscriptTruncationKey,
                [ConsequenceType.TranscriptAblation] = TranscriptAblatioinKey,
                [ConsequenceType.Unknown] = UnknownKey,
                [ConsequenceType.UpstreamGeneVariant] = UpstreamGeneVariantKey,
                [ConsequenceType.ShortTandemRepeatChange] = ShortTandemRepeatChangeKey,
                [ConsequenceType.ShortTandemRepeatExpansion] = ShortTandemRepeatExpansionKey,
                [ConsequenceType.ShortTandemRepeatContraction] = ShortTandemRepeatContractionKey,
                [ConsequenceType.TranscriptVariant] = TranscriptVariantKey,
                [ConsequenceType.GeneFusion] = GeneFusionKey
            };
        }


        /// <summary>
        /// adds the CNV consequences
        /// </summary>
        private void AssignCnvTypes(VariantType internalCopyNumberType)
        {
            // CopyNumber*
            _consequences.Add(_variantEffect.EvaluateCopyNumberConsequence(internalCopyNumberType));
        }

        /// <summary>
        /// determines the variant's functional consequence
        /// </summary>
        public void DetermineVariantEffects(VariantType internalCopyNumberType)
        {
	        var strConsequenceType = _variantEffect.GetStrConsequenceType();
	        if (strConsequenceType != ConsequenceType.Unknown)
	        {
				_consequences.Clear();
		        _consequences.Add(strConsequenceType);
		        return;
	        }

			GetTier1Types();
            if (_consequences.Count == 0) GetTier2Types();
            if (_consequences.Count == 0) GetTier3Types();

            if (internalCopyNumberType != VariantType.unknown) AssignCnvTypes(internalCopyNumberType);
	        
            AssignGeneFusion();

            if (_consequences.Count == 0) _consequences.Add(ConsequenceType.TranscriptVariant);
        }

        
        /// <summary>
        /// determines the flanking variant's functional consequence
        /// </summary>
        public void DetermineFlankingVariantEffects(bool isDownstreamVariant, VariantType internalCopyNumberType)
        {
            // add the appropriate flanking variant consequence
            _consequences.Add(isDownstreamVariant
                ? ConsequenceType.DownstreamGeneVariant
                : ConsequenceType.UpstreamGeneVariant);

            // FeatureElongation
            if (_variantEffect.HasTranscriptElongation()) _consequences.Add(ConsequenceType.FeatureElongation);

            // TranscriptTruncation
            if (_variantEffect.HasTranscriptTruncation()) _consequences.Add(ConsequenceType.TranscriptTruncation);

            // CopyNumber
            if (internalCopyNumberType != VariantType.unknown) AssignCnvTypes(internalCopyNumberType);
        }

        /// <summary>
        /// determines the regulatory region variant's functional consequence
        /// </summary>
        public void DetermineRegulatoryVariantEffects(ReferenceAnnotationInterval feature, VariantType vt, int refBegin, int refEnd, bool isSv, VariantType internalCopyNumberType)
        {
            var regulatoryVariantEffects = new FeatureVariantEffects(feature, vt, refBegin, refEnd, isSv, internalCopyNumberType);

            // RegulatoryRegionAmplification
            if (regulatoryVariantEffects.Amplification()) _consequences.Add(ConsequenceType.RegulatoryRegionAmplification);

            // RegulatoryRegionAblation
            if (regulatoryVariantEffects.Ablation()) _consequences.Add(ConsequenceType.RegulatoryRegionAblation);

            // RegulatoryRegionVariant
            _consequences.Add(ConsequenceType.RegulatoryRegionVariant);
        }

        private void AssignGeneFusion()
        {
            if (!_variantEffect.IsGeneFusion()) return;

            _consequences.Add(ConsequenceType.GeneFusion);

        }

        /// <summary>
        /// populates the consequences list with tier 1 consequences if found (NOTE: Tests are done in rank order)
        /// </summary>
        private void GetTier1Types()
        {
            // TranscriptAblation
            if (_variantEffect.HasTranscriptAblation()) _consequences.Add(ConsequenceType.TranscriptAblation);

            // TranscriptAmplification
            if (_variantEffect.HasTranscriptAmplification()) _consequences.Add(ConsequenceType.TranscriptAmplification);

        }

        /// <summary>
        /// populates the consequences list with tier 2 consequences if found (NOTE: Tests are done in rank order)
        /// </summary>
        private void GetTier2Types()
        {
            // MatureMirnaVariant
            if (_variantEffect.IsMatureMirnaVariant()) _consequences.Add(ConsequenceType.MatureMirnaVariant);

            // TfbsAblation
            // TfbsAmplification
            // TfBindingSiteVariant
        }

        /// <summary>
        /// populates the consequences list with tier 3 consequences if found (NOTE: Tests are done in rank order)
        /// </summary>
        private void GetTier3Types()
        {
            // SpliceDonorVariant
            if (_variantEffect.IsSpliceDonorVariant()) _consequences.Add(ConsequenceType.SpliceDonorVariant);

            // SpliceAcceptorVariant
            if (_variantEffect.IsSpliceAcceptorVariant()) _consequences.Add(ConsequenceType.SpliceAcceptorVariant);

            // StopGained
            if (_variantEffect.IsStopGained()) _consequences.Add(ConsequenceType.StopGained);

            // FrameshiftVariant
            if (_variantEffect.IsFrameshiftVariant()) _consequences.Add(ConsequenceType.FrameshiftVariant);

            // StopLost
            if (_variantEffect.IsStopLost()) _consequences.Add(ConsequenceType.StopLost);

            // StartLost
            if (_variantEffect.IsStartLost()) _consequences.Add(ConsequenceType.StartLost);

            // InframeInsertion
            if (_variantEffect.IsInframeInsertion()) _consequences.Add(ConsequenceType.InframeInsertion);

            // InframeDeletion
            if (_variantEffect.IsInframeDeletion()) _consequences.Add(ConsequenceType.InframeDeletion);

            // MissenseVariant
            if (_variantEffect.IsMissenseVariant()) _consequences.Add(ConsequenceType.MissenseVariant);

            // ProteinAlteringVariant
            if (_variantEffect.IsProteinAlteringVariant()) _consequences.Add(ConsequenceType.ProteinAlteringVariant);

            // SpliceRegionVariant
            if (_variantEffect.IsSpliceRegionVariant()) _consequences.Add(ConsequenceType.SpliceRegionVariant);

            // IncompleteTerminalCodonVariant
            if (_variantEffect.IsIncompleteTerminalCodonVariant()) _consequences.Add(ConsequenceType.IncompleteTerminalCodonVariant);

            // StopRetainedVariant
            if (_variantEffect.IsStopRetained()) _consequences.Add(ConsequenceType.StopRetainedVariant);

            // SynonymousVariant
            if (_variantEffect.IsSynonymousVariant()) _consequences.Add(ConsequenceType.SynonymousVariant);

            // CodingSequenceVariant
            if (_variantEffect.IsCodingSequenceVariant()) _consequences.Add(ConsequenceType.CodingSequenceVariant);

            // FivePrimeUtrVariant
            if (_variantEffect.IsFivePrimeUtrVariant()) _consequences.Add(ConsequenceType.FivePrimeUtrVariant);

            // ThreePrimeUtrVariant
            if (_variantEffect.IsThreePrimeUtrVariant()) _consequences.Add(ConsequenceType.ThreePrimeUtrVariant);

            // NonCodingTranscriptExonVariant
            if (_variantEffect.IsNonCodingTranscriptExonVariant()) _consequences.Add(ConsequenceType.NonCodingTranscriptExonVariant);

            // IntronVariant
            if (_variantEffect.IsWithinIntron()) _consequences.Add(ConsequenceType.IntronVariant);

            // NonsenseMediatedDecayTranscriptVariant
            if (_variantEffect.IsNonsenseMediatedDecayTranscriptVariant()) _consequences.Add(ConsequenceType.NonsenseMediatedDecayTranscriptVariant);

            // NonCodingTranscriptVariant
            if (_variantEffect.IsNonCodingTranscriptVariant()) _consequences.Add(ConsequenceType.NonCodingTranscriptVariant);

            // FeatureElongation
            if (_variantEffect.HasTranscriptElongation()) _consequences.Add(ConsequenceType.FeatureElongation);

            // TranscriptTruncation
            if (_variantEffect.HasTranscriptTruncation()) _consequences.Add(ConsequenceType.TranscriptTruncation);
        }

        /// <summary>
        /// returns an array of strings containing the consequence identifiers
        /// </summary>
        public string[] GetConsequenceStrings()
        {
            var consequenceStrings = new string[_consequences.Count];

            for (var i = 0; i < _consequences.Count; i++)
            {
                consequenceStrings[i] = _consequenceDescriptors[_consequences[i]];
            }

            return consequenceStrings;
        }
    }
}
