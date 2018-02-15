using Moq;
using VariantAnnotation.AnnotatedPositions.Consequence;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Positions;
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
            var consequence = new Consequences();
            consequence.DetermineFlankingVariantEffects(isDownStreamVariant);
            var observedConsequences = consequence.GetConsequences();
            Assert.Single(observedConsequences);
            Assert.Equal(expectedConsequence, observedConsequences[0]);
        }

        [Fact]
        public void DetermineSmallVariantEffects_tier1()
        {
            var featureEffect = new Mock<IFeatureVariantEffects>();
            featureEffect.Setup(x => x.Ablation()).Returns(true);
            featureEffect.Setup(x => x.Amplification()).Returns(true);

            var variantEffect = new Mock<IVariantEffect>();
            var consequence = new Consequences(variantEffect.Object, featureEffect.Object);
            consequence.DetermineSmallVariantEffects();
            var observedConsequences = consequence.GetConsequences();
            Assert.Equal(2, observedConsequences.Count);
            Assert.Equal(ConsequenceTag.transcript_ablation, observedConsequences[0]);
            Assert.Equal(ConsequenceTag.transcript_amplification, observedConsequences[1]);
        }

        [Fact]
        public void DetermineSmallVariantEffects_tier2()
        {
            var featureEffect = new Mock<IFeatureVariantEffects>();
            featureEffect.Setup(x => x.Ablation()).Returns(false);
            featureEffect.Setup(x => x.Amplification()).Returns(false);

            var variantEffect = new Mock<IVariantEffect>();
            variantEffect.Setup(x => x.IsMatureMirnaVariant()).Returns(true);
            var consequence = new Consequences(variantEffect.Object, featureEffect.Object);
            consequence.DetermineSmallVariantEffects();
            var observedConsequences = consequence.GetConsequences();
            Assert.Single(observedConsequences);
            Assert.Equal(ConsequenceTag.mature_miRNA_variant, observedConsequences[0]);
        }


        [Fact]
        public void DetermineSmallVariantEffects_tier3()
        {
            var cache = new VariantEffectCache();
            cache.Add(ConsequenceTag.mature_miRNA_variant, false);

            cache.Add(ConsequenceTag.splice_donor_variant, true);
            cache.Add(ConsequenceTag.splice_acceptor_variant, true);
            cache.Add(ConsequenceTag.stop_gained, true);
            cache.Add(ConsequenceTag.frameshift_variant, true);
            cache.Add(ConsequenceTag.stop_lost, true);
            cache.Add(ConsequenceTag.start_lost, true);
            cache.Add(ConsequenceTag.inframe_insertion, true);
            cache.Add(ConsequenceTag.inframe_deletion, true);
            cache.Add(ConsequenceTag.missense_variant, true);
            cache.Add(ConsequenceTag.protein_altering_variant, true);
            cache.Add(ConsequenceTag.splice_region_variant, true);
            cache.Add(ConsequenceTag.incomplete_terminal_codon_variant, true);
            cache.Add(ConsequenceTag.stop_retained_variant, true);
            cache.Add(ConsequenceTag.synonymous_variant, true);
            cache.Add(ConsequenceTag.coding_sequence_variant, true);
            cache.Add(ConsequenceTag.five_prime_UTR_variant, true);
            cache.Add(ConsequenceTag.three_prime_UTR_variant, true);
            cache.Add(ConsequenceTag.non_coding_transcript_exon_variant, true);
            cache.Add(ConsequenceTag.intron_variant, true);
            cache.Add(ConsequenceTag.NMD_transcript_variant, true);
            cache.Add(ConsequenceTag.non_coding_transcript_variant, true);


            var simpleVariant = new Mock<ISimpleVariant>();
            simpleVariant.SetupGet(x => x.RefAllele).Returns("G");
            simpleVariant.SetupGet(x => x.AltAllele).Returns("C");

            var positionalEffect = new TranscriptPositionalEffect
            {
                IsWithinIntron = true
            };
            var variantEffect = new VariantEffect(positionalEffect, simpleVariant.Object, null, null, null, null, null, null, cache);

            var featureEffect = new Mock<IFeatureVariantEffects>();
            featureEffect.Setup(x => x.Ablation()).Returns(false);
            featureEffect.Setup(x => x.Amplification()).Returns(false);
            featureEffect.Setup(x => x.Truncation()).Returns(true);
            featureEffect.Setup(x => x.Elongation()).Returns(true);

            var consequence = new Consequences(variantEffect, featureEffect.Object);

            consequence.DetermineSmallVariantEffects();
            var observedConsequence = consequence.GetConsequences();
            Assert.Equal(ConsequenceTag.splice_donor_variant, observedConsequence[0]);
            Assert.Equal(ConsequenceTag.splice_acceptor_variant, observedConsequence[1]);
            Assert.Equal(ConsequenceTag.stop_gained, observedConsequence[2]);
            Assert.Equal(ConsequenceTag.frameshift_variant, observedConsequence[3]);
            Assert.Equal(ConsequenceTag.stop_lost, observedConsequence[4]);
            Assert.Equal(ConsequenceTag.start_lost, observedConsequence[5]);
            Assert.Equal(ConsequenceTag.inframe_insertion, observedConsequence[6]);
            Assert.Equal(ConsequenceTag.inframe_deletion, observedConsequence[7]);
            Assert.Equal(ConsequenceTag.missense_variant, observedConsequence[8]);
            Assert.Equal(ConsequenceTag.protein_altering_variant, observedConsequence[9]);
            Assert.Equal(ConsequenceTag.splice_region_variant, observedConsequence[10]);
            Assert.Equal(ConsequenceTag.incomplete_terminal_codon_variant, observedConsequence[11]);

            Assert.Equal(ConsequenceTag.stop_retained_variant, observedConsequence[12]);
            Assert.Equal(ConsequenceTag.synonymous_variant, observedConsequence[13]);
            Assert.Equal(ConsequenceTag.coding_sequence_variant, observedConsequence[14]);
            Assert.Equal(ConsequenceTag.five_prime_UTR_variant, observedConsequence[15]);
            Assert.Equal(ConsequenceTag.three_prime_UTR_variant, observedConsequence[16]);
            Assert.Equal(ConsequenceTag.non_coding_transcript_exon_variant, observedConsequence[17]);
            Assert.Equal(ConsequenceTag.intron_variant, observedConsequence[18]);
            Assert.Equal(ConsequenceTag.NMD_transcript_variant, observedConsequence[19]);
            Assert.Equal(ConsequenceTag.non_coding_transcript_variant, observedConsequence[20]);
        }


        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void DetermineRegularoryVariantEffects(bool isAmplification, bool isAblation)
        {
            var featureEffect = new Mock<IFeatureVariantEffects>();
            featureEffect.Setup(x => x.Ablation()).Returns(isAblation);
            featureEffect.Setup(x => x.Amplification()).Returns(isAmplification);

            var consequence = new Consequences(null, featureEffect.Object);
            consequence.DetermineRegulatoryVariantEffects();
            var observedConsequences = consequence.GetConsequences();

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

        [Theory]
        [InlineData(true, false, false, false, new[] { ConsequenceTag.transcript_ablation })]
        [InlineData(false, true, false, false, new[] { ConsequenceTag.transcript_amplification })]
        [InlineData(true, false, true, false, new[] { ConsequenceTag.transcript_ablation })]
        [InlineData(false, false, true, false, new[] { ConsequenceTag.feature_elongation })]
        [InlineData(false, false, false, true, new[] { ConsequenceTag.transcript_truncation })]
        public void DetermineStructuralVariantEffect(bool isAblation, bool isAmplification, bool isElongation,
            bool isTruncation, ConsequenceTag[] expectedConsequences)
        {

            var featureEffect = new Mock<IFeatureVariantEffects>();
            featureEffect.Setup(x => x.Ablation()).Returns(isAblation);
            featureEffect.Setup(x => x.Amplification()).Returns(isAmplification);
            featureEffect.Setup(x => x.Elongation()).Returns(isElongation);
            featureEffect.Setup(x => x.Truncation()).Returns(isTruncation);

            var consequence = new Consequences(null, featureEffect.Object);
            consequence.DetermineStructuralVariantEffect(VariantType.unknown, false);
            var observedConsequences = consequence.GetConsequences().ToArray();

            Assert.Equal(expectedConsequences, observedConsequences);
        }
    }
}