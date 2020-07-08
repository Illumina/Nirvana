using System.Collections.Generic;
using Moq;
using VariantAnnotation.AnnotatedPositions.Consequence;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class ConsequenceTests
    {
        [Theory]
        [InlineData(false, ConsequenceTag.upstream_gene_variant)]
        [InlineData(true, ConsequenceTag.downstream_gene_variant)]
        public void DetermineFlankingVariantEffects(bool isDownStreamVariant, ConsequenceTag expectedConsequence)
        {
            List<ConsequenceTag> observedConsequences = Consequences.DetermineFlankingVariantEffects(isDownStreamVariant);
            Assert.Single(observedConsequences);
            Assert.Equal(expectedConsequence, observedConsequences[0]);
        }

        [Theory]
        [InlineData(VariantType.deletion,         true,  false, ConsequenceTag.transcript_ablation)]
        [InlineData(VariantType.copy_number_gain, false, true,  ConsequenceTag.transcript_amplification)]
        public void DetermineSmallVariantEffects_Tier1(VariantType variantType, bool isAblation, bool isAmplification, ConsequenceTag expectedResult)
        {
            var featureEffect = new Mock<IFeatureVariantEffects>();
            featureEffect.Setup(x => x.Ablation()).Returns(isAblation);
            featureEffect.Setup(x => x.Amplification()).Returns(isAmplification);

            var variantEffect = new Mock<IVariantEffect>();

            // make sure these tier 2 effects don't show up
            featureEffect.Setup(x => x.Elongation()).Returns(true);
            variantEffect.Setup(x => x.IsMatureMirnaVariant()).Returns(true);

            var consequence = new Consequences(variantType, variantEffect.Object, featureEffect.Object);
            consequence.DetermineSmallVariantEffects();

            List<ConsequenceTag> observedConsequences = consequence.GetConsequences();
            Assert.Contains(expectedResult, observedConsequences);
        }

        [Fact]
        public void DetermineSmallVariantEffects_Tier2()
        {
            var featureEffect = new Mock<IFeatureVariantEffects>();
            featureEffect.Setup(x => x.Ablation()).Returns(false);
            featureEffect.Setup(x => x.Amplification()).Returns(false);

            var variantEffect = new Mock<IVariantEffect>();
            variantEffect.Setup(x => x.IsMatureMirnaVariant()).Returns(true);

            // make sure these tier 3 effects don't show up
            variantEffect.Setup(x => x.IsStartLost()).Returns(true);

            var consequence = new Consequences(VariantType.SNV, variantEffect.Object, featureEffect.Object);
            consequence.DetermineSmallVariantEffects();

            List<ConsequenceTag> observedConsequences = consequence.GetConsequences();
            Assert.Single(observedConsequences);
            Assert.Equal(ConsequenceTag.mature_miRNA_variant, observedConsequences[0]);
        }

        [Theory]
        [InlineData(VariantType.SNV,                           true)]
        [InlineData(VariantType.insertion,                     true)]
        [InlineData(VariantType.deletion,                      true)]
        [InlineData(VariantType.indel,                         true)]
        [InlineData(VariantType.MNV,                           true)]
        [InlineData(VariantType.duplication,                   false)] // no change
        [InlineData(VariantType.complex_structural_alteration, true)]
        [InlineData(VariantType.structural_alteration,         true)]
        [InlineData(VariantType.tandem_duplication,            false)] // no change
        [InlineData(VariantType.translocation_breakend,        true)]
        [InlineData(VariantType.inversion,                     true)]
        [InlineData(VariantType.short_tandem_repeat_variation, true)]
        [InlineData(VariantType.copy_number_variation,         false)] // no change
        [InlineData(VariantType.copy_number_loss,              false)] // no change
        [InlineData(VariantType.copy_number_gain,              false)] // no change
        [InlineData(VariantType.run_of_homozygosity,           false)] // no change
        public void NeedsTranscriptVariant_NoConsequences_EvaluateByVariantType(VariantType variantType, bool expectedResult)
        {
            var  consequences   = new List<ConsequenceTag>();
            bool observedResult = Consequences.NeedsTranscriptVariant(variantType, consequences);
            Assert.Equal(expectedResult, observedResult);
        }
        
        [Theory]
        [InlineData(ConsequenceTag.transcript_ablation,      true)]  // parallel
        [InlineData(ConsequenceTag.transcript_amplification, false)] // parallel, no change
        public void NeedsTranscriptVariant_Tier1(ConsequenceTag consequence, bool expectedResult)
        {
            var  consequences   = new List<ConsequenceTag> {consequence};
            bool observedResult = Consequences.NeedsTranscriptVariant(VariantType.unknown, consequences);
            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void NeedsTranscriptVariant_Tier2_MatureMirnaVariant()
        {
            var  consequences   = new List<ConsequenceTag> {ConsequenceTag.mature_miRNA_variant};
            bool observedResult = Consequences.NeedsTranscriptVariant(VariantType.unknown, consequences);
            Assert.False(observedResult);
        }

        [Theory]
        [InlineData(ConsequenceTag.splice_donor_variant,               false)]
        [InlineData(ConsequenceTag.splice_acceptor_variant,            false)]
        [InlineData(ConsequenceTag.stop_gained,                        false)]
        [InlineData(ConsequenceTag.frameshift_variant,                 false)]
        [InlineData(ConsequenceTag.stop_lost,                          false)]
        [InlineData(ConsequenceTag.start_lost,                         false)]
        [InlineData(ConsequenceTag.inframe_insertion,                  false)]
        [InlineData(ConsequenceTag.inframe_deletion,                   false)]
        [InlineData(ConsequenceTag.missense_variant,                   false)]
        [InlineData(ConsequenceTag.protein_altering_variant,           false)]
        [InlineData(ConsequenceTag.splice_region_variant,              false)]
        [InlineData(ConsequenceTag.incomplete_terminal_codon_variant,  false)]
        [InlineData(ConsequenceTag.start_retained_variant,             false)]
        [InlineData(ConsequenceTag.stop_retained_variant,              false)]
        [InlineData(ConsequenceTag.synonymous_variant,                 false)]
        [InlineData(ConsequenceTag.coding_sequence_variant,            false)]
        [InlineData(ConsequenceTag.five_prime_UTR_variant,             false)]
        [InlineData(ConsequenceTag.three_prime_UTR_variant,            false)]
        [InlineData(ConsequenceTag.non_coding_transcript_exon_variant, false)]
        [InlineData(ConsequenceTag.intron_variant,                     false)]
        [InlineData(ConsequenceTag.NMD_transcript_variant,             false)]
        [InlineData(ConsequenceTag.non_coding_transcript_variant,      false)]
        [InlineData(ConsequenceTag.feature_elongation,                 true)] // parallel
        [InlineData(ConsequenceTag.feature_truncation,                 true)] // parallel
        public void NeedsTranscriptVariant_Tier3(ConsequenceTag consequence, bool expectedResult)
        {
            var  consequences   = new List<ConsequenceTag> {consequence};
            bool observedResult = Consequences.NeedsTranscriptVariant(VariantType.unknown, consequences);
            Assert.Equal(expectedResult, observedResult);
        }

        [Theory]
        [InlineData(ConsequenceTag.feature_elongation,                true)]  // parallel
        [InlineData(ConsequenceTag.feature_truncation,                true)]  // parallel
        [InlineData(ConsequenceTag.five_prime_duplicated_transcript,  false)] // child
        [InlineData(ConsequenceTag.three_prime_duplicated_transcript, false)] // child
        public void NeedsTranscriptVariant_Tier2_SV(ConsequenceTag consequence, bool expectedResult)
        {
            var  consequences   = new List<ConsequenceTag> {consequence};
            bool observedResult = Consequences.NeedsTranscriptVariant(VariantType.unknown, consequences);
            Assert.Equal(expectedResult, observedResult);
        }

        [Theory]
        [InlineData(ConsequenceTag.copy_number_increase, false)] // no change
        [InlineData(ConsequenceTag.copy_number_decrease, false)] // no change
        [InlineData(ConsequenceTag.copy_number_change,   false)] // no change
        public void NeedsTranscriptVariant_CNV(ConsequenceTag consequence, bool expectedResult)
        {
            var  consequences   = new List<ConsequenceTag> {consequence};
            bool observedResult = Consequences.NeedsTranscriptVariant(VariantType.unknown, consequences);
            Assert.Equal(expectedResult, observedResult);
        }

        [Theory]
        [InlineData(ConsequenceTag.short_tandem_repeat_change,      true)] // parallel
        [InlineData(ConsequenceTag.short_tandem_repeat_expansion,   true)] // parallel
        [InlineData(ConsequenceTag.short_tandem_repeat_contraction, true)] // parallel
        public void NeedsTranscriptVariant_STR(ConsequenceTag consequence, bool expectedResult)
        {
            var  consequences   = new List<ConsequenceTag> {consequence};
            bool observedResult = Consequences.NeedsTranscriptVariant(VariantType.unknown, consequences);
            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void DetermineSmallVariantEffects_Tier3()
        {
            var cache = new VariantEffectCache();
            cache.Add(ConsequenceTag.mature_miRNA_variant, false);

            cache.Add(ConsequenceTag.splice_donor_variant,               true);
            cache.Add(ConsequenceTag.splice_acceptor_variant,            true);
            cache.Add(ConsequenceTag.stop_gained,                        true);
            cache.Add(ConsequenceTag.frameshift_variant,                 true);
            cache.Add(ConsequenceTag.stop_lost,                          true);
            cache.Add(ConsequenceTag.start_lost,                         true);
            cache.Add(ConsequenceTag.inframe_insertion,                  true);
            cache.Add(ConsequenceTag.inframe_deletion,                   true);
            cache.Add(ConsequenceTag.missense_variant,                   true);
            cache.Add(ConsequenceTag.protein_altering_variant,           true);
            cache.Add(ConsequenceTag.splice_region_variant,              true);
            cache.Add(ConsequenceTag.incomplete_terminal_codon_variant,  true);
            cache.Add(ConsequenceTag.stop_retained_variant,              true);
            cache.Add(ConsequenceTag.synonymous_variant,                 true);
            cache.Add(ConsequenceTag.coding_sequence_variant,            true);
            cache.Add(ConsequenceTag.five_prime_UTR_variant,             true);
            cache.Add(ConsequenceTag.three_prime_UTR_variant,            true);
            cache.Add(ConsequenceTag.non_coding_transcript_exon_variant, true);
            cache.Add(ConsequenceTag.intron_variant,                     true);
            cache.Add(ConsequenceTag.NMD_transcript_variant,             true);
            cache.Add(ConsequenceTag.non_coding_transcript_variant,      true);

            var simpleVariant = new Mock<ISimpleVariant>();
            simpleVariant.SetupGet(x => x.RefAllele).Returns("G");
            simpleVariant.SetupGet(x => x.AltAllele).Returns("C");

            var positionalEffect = new TranscriptPositionalEffect {IsWithinIntron = true};
            var variantEffect    = new VariantEffect(positionalEffect, simpleVariant.Object, null, null, null, null, null, null, null, null, cache);

            var featureEffect = new Mock<IFeatureVariantEffects>();
            featureEffect.Setup(x => x.Ablation()).Returns(false);
            featureEffect.Setup(x => x.Amplification()).Returns(false);
            featureEffect.Setup(x => x.Truncation()).Returns(true);
            featureEffect.Setup(x => x.Elongation()).Returns(true);

            var consequence = new Consequences(VariantType.SNV, variantEffect, featureEffect.Object);

            consequence.DetermineSmallVariantEffects();
            List<ConsequenceTag> observedConsequence = consequence.GetConsequences();
            Assert.Equal(ConsequenceTag.splice_donor_variant,               observedConsequence[0]);
            Assert.Equal(ConsequenceTag.splice_acceptor_variant,            observedConsequence[1]);
            Assert.Equal(ConsequenceTag.stop_gained,                        observedConsequence[2]);
            Assert.Equal(ConsequenceTag.frameshift_variant,                 observedConsequence[3]);
            Assert.Equal(ConsequenceTag.stop_lost,                          observedConsequence[4]);
            Assert.Equal(ConsequenceTag.start_lost,                         observedConsequence[5]);
            Assert.Equal(ConsequenceTag.inframe_insertion,                  observedConsequence[6]);
            Assert.Equal(ConsequenceTag.inframe_deletion,                   observedConsequence[7]);
            Assert.Equal(ConsequenceTag.missense_variant,                   observedConsequence[8]);
            Assert.Equal(ConsequenceTag.protein_altering_variant,           observedConsequence[9]);
            Assert.Equal(ConsequenceTag.splice_region_variant,              observedConsequence[10]);
            Assert.Equal(ConsequenceTag.incomplete_terminal_codon_variant,  observedConsequence[11]);
            Assert.Equal(ConsequenceTag.stop_retained_variant,              observedConsequence[12]);
            Assert.Equal(ConsequenceTag.synonymous_variant,                 observedConsequence[13]);
            Assert.Equal(ConsequenceTag.coding_sequence_variant,            observedConsequence[14]);
            Assert.Equal(ConsequenceTag.five_prime_UTR_variant,             observedConsequence[15]);
            Assert.Equal(ConsequenceTag.three_prime_UTR_variant,            observedConsequence[16]);
            Assert.Equal(ConsequenceTag.non_coding_transcript_exon_variant, observedConsequence[17]);
            Assert.Equal(ConsequenceTag.intron_variant,                     observedConsequence[18]);
            Assert.Equal(ConsequenceTag.NMD_transcript_variant,             observedConsequence[19]);
            Assert.Equal(ConsequenceTag.non_coding_transcript_variant,      observedConsequence[20]);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void DetermineRegulatoryVariantEffects(bool isAmplification, bool isAblation)
        {
            var featureEffect = new Mock<IFeatureVariantEffects>();
            featureEffect.Setup(x => x.Ablation()).Returns(isAblation);
            featureEffect.Setup(x => x.Amplification()).Returns(isAmplification);

            var consequence = new Consequences(VariantType.unknown, null, featureEffect.Object);
            consequence.DetermineRegulatoryVariantEffects();
            List<ConsequenceTag> observedConsequences = consequence.GetConsequences();

            Assert.Contains(ConsequenceTag.regulatory_region_variant, observedConsequences);
            if (isAblation)
            {
                Assert.Contains(ConsequenceTag.regulatory_region_ablation, observedConsequences);
            }
            else
            {
                Assert.DoesNotContain(ConsequenceTag.regulatory_region_ablation, observedConsequences);
            }

            if (isAmplification)
            {
                Assert.Contains(ConsequenceTag.regulatory_region_amplification, observedConsequences);
            }
            else
            {
                Assert.DoesNotContain(ConsequenceTag.regulatory_region_amplification, observedConsequences);
            }
        }

        public static IEnumerable<object[]> SvTheoryParameters()
        {
            yield return new object[] {VariantType.copy_number_loss,   true,  false, false, false, false, false, new[] {ConsequenceTag.transcript_ablation, ConsequenceTag.copy_number_decrease}};
            yield return new object[] {VariantType.copy_number_gain,   false, true,  false, false, false, false, new[] {ConsequenceTag.transcript_amplification, ConsequenceTag.copy_number_increase}};
            yield return new object[] {VariantType.deletion,           true,  false, true,  false, false, false, new[] {ConsequenceTag.transcript_ablation, ConsequenceTag.transcript_variant}};
            yield return new object[] {VariantType.duplication,        false, true,  true,  false, false, false, new[] {ConsequenceTag.transcript_amplification}};
            yield return new object[] {VariantType.tandem_duplication, false, false, true,  false, false, false, new[] {ConsequenceTag.feature_elongation, ConsequenceTag.transcript_variant}};
            yield return new object[] {VariantType.copy_number_loss,   false, false, false, true,  false, false, new[] {ConsequenceTag.feature_truncation, ConsequenceTag.copy_number_decrease}};
            yield return new object[] {VariantType.copy_number_gain,   false, false, false, false, true,  false, new[] {ConsequenceTag.five_prime_duplicated_transcript, ConsequenceTag.copy_number_increase}};
            yield return new object[] {VariantType.duplication,        false, false, false, false, false, true,  new[] {ConsequenceTag.three_prime_duplicated_transcript}};
        }

        [Theory]
        [MemberData(nameof(SvTheoryParameters))]
        public void DetermineStructuralVariantEffect(VariantType variantType, bool isAblation, bool isAmplification, bool isElongation,
                                                     bool isTruncation, bool isFivePrimeDuplicatedTranscript, bool isThreePrimeDuplicatedTranscript,
                                                     ConsequenceTag[] expectedResults)
        {
            IFeatureVariantEffects featureVariantEffects = GetFeatureVariantEffects(isAblation, isAmplification, isTruncation, isElongation,
                isFivePrimeDuplicatedTranscript, isThreePrimeDuplicatedTranscript);

            var variant = new Variant(null, 0, 0, null, null, variantType, null, false, false, false, null, AnnotationBehavior.StructuralVariants,
                true);

            var consequence = new Consequences(variantType, null, featureVariantEffects);
            consequence.DetermineStructuralVariantEffect(variant);
            ConsequenceTag[] observedResults = consequence.GetConsequences().ToArray();

            Assert.Equal(expectedResults, observedResults);
        }

        private static IFeatureVariantEffects GetFeatureVariantEffects(bool isAblation,   bool isAmplification,
                                                                       bool isTruncation, bool isElongation,
                                                                       bool isFivePrimeDuplicatedTranscript,
                                                                       bool isThreePrimeDuplicatedTranscript)
        {
            var featureEffectsMock = new Mock<IFeatureVariantEffects>();
            featureEffectsMock.Setup(x => x.Ablation()).Returns(isAblation);
            featureEffectsMock.Setup(x => x.Amplification()).Returns(isAmplification);
            featureEffectsMock.Setup(x => x.Elongation()).Returns(isElongation);
            featureEffectsMock.Setup(x => x.Truncation()).Returns(isTruncation);
            featureEffectsMock.Setup(x => x.FivePrimeDuplicatedTranscript()).Returns(isFivePrimeDuplicatedTranscript);
            featureEffectsMock.Setup(x => x.ThreePrimeDuplicatedTranscript()).Returns(isThreePrimeDuplicatedTranscript);
            return featureEffectsMock.Object;
        }
    }
}