using Cache.Data;
using Intervals;
using UnitTests.TestUtilities;
using VariantAnnotation.AnnotatedPositions;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class TranscriptPositionalEffectTests
    {
        private readonly Transcript         _forwardTranscript; // use info from "ENST00000455979.1" with modification
        private readonly Transcript         _reverseTranscript; // use info from "ENST00000385042"
        private readonly TranscriptRegion[] _otherTranscriptRegions;

        public TranscriptPositionalEffectTests()
        {
            const int start = 874655;
            const int end   = 879639;

            _otherTranscriptRegions = new TranscriptRegion[]
            {
                new(200, 300, 1, 186, TranscriptRegionType.Exon, 1, null),
                new(301, 400, 186, 187, TranscriptRegionType.Intron, 1, null),
                new(401, 699, 187, 349, TranscriptRegionType.Exon, 2, null),
                new(700, 709, 359, 360, TranscriptRegionType.Intron, 2, null),
                new(710, 800, 350, 465, TranscriptRegionType.Exon, 3, null)
            };

            var forwardTranscriptRegions = new TranscriptRegion[]
            {
                new(874655, 874840, 1, 186, TranscriptRegionType.Exon, 1, null),
                new(874841, 876523, 186, 187, TranscriptRegionType.Intron, 1, null),
                new(876524, 876686, 187, 349, TranscriptRegionType.Exon, 2, null),
                new(876687, 877515, 349, 350, TranscriptRegionType.Intron, 2, null),
                new(877516, 877631, 350, 465, TranscriptRegionType.Exon, 3, null),
                new(877632, 877789, 465, 466, TranscriptRegionType.Intron, 3, null),
                new(877790, 877868, 466, 544, TranscriptRegionType.Exon, 4, null),
                new(877869, 877938, 544, 545, TranscriptRegionType.Intron, 4, null),
                new(877939, 878438, 545, 1044, TranscriptRegionType.Exon, 5, null),
                new(878439, 878632, 1044, 1045, TranscriptRegionType.Intron, 5, null),
                new(878633, 878757, 1045, 1169, TranscriptRegionType.Exon, 6, null),
                new(878758, 879077, 1169, 1170, TranscriptRegionType.Intron, 6, null),
                new(879078, 879639, 1170, 1731, TranscriptRegionType.Exon, 7, null)
            };

            var reverseTranscriptRegions = new TranscriptRegion[]
            {
                new(3477259, 3477354, 1, 96, TranscriptRegionType.Exon, 1, null)
            };

            var codingRegion = new CodingRegion(874655, 879533, 1, 1625, "NP_123", "MRD*", 0, 0, 0, null, null);

            var gene = new Gene("123", "ENSG123", false, null);

            _forwardTranscript = new(ChromosomeUtilities.Chr1, start, end, "ABC", BioType.mRNA, false, Source.RefSeq,
                gene, forwardTranscriptRegions, new string('A', 1731), codingRegion);

            _reverseTranscript = new(ChromosomeUtilities.Chr1, 3477259, 3477354, "DEF", BioType.miRNA, false,
                Source.RefSeq, gene, reverseTranscriptRegions, "ACGT", null);
        }

        [Fact]
        public void DetermineIntronicEffect_NullIntrons()
        {
            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineIntronicEffect(null, new Interval(400, 400), VariantType.SNV);

            Assert.False(positionalEffect.IsEndSpliceSite);
            Assert.False(positionalEffect.IsStartSpliceSite);
            Assert.False(positionalEffect.IsWithinFrameshiftIntron);
            Assert.False(positionalEffect.IsWithinIntron);
            Assert.False(positionalEffect.IsWithinSpliceSiteRegion);
            Assert.False(positionalEffect.HasExonOverlap);
            Assert.False(positionalEffect.AfterCoding);
            Assert.False(positionalEffect.BeforeCoding);
            Assert.False(positionalEffect.WithinCdna);
            Assert.False(positionalEffect.WithinCds);
            Assert.False(positionalEffect.HasFrameShift);
            Assert.False(positionalEffect.IsCoding);
        }

        [Fact]
        public void DetermineIntronicEffect_NotWithinFrameshiftIntron()
        {
            var transcriptRegions = new TranscriptRegion[]
            {
                new(201342300, 201342340, 1, 186, TranscriptRegionType.Exon, 1, null),
                new(201342340, 201342343, 186, 187, TranscriptRegionType.Intron, 1, null),
                new(201342344, 201342400, 187, 349, TranscriptRegionType.Exon, 2, null)
            };

            IInterval variant          = new Interval(201342344, 201342344);
            var       positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineIntronicEffect(transcriptRegions, variant, VariantType.SNV);

            Assert.True(positionalEffect.IsWithinSpliceSiteRegion);
        }

        [Fact]
        public void DetermineIntronicEffect_IsEndSpliceSite()
        {
            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineIntronicEffect(_otherTranscriptRegions, new Interval(400, 400), VariantType.SNV);
            Assert.True(positionalEffect.IsEndSpliceSite);
        }

        [Fact]
        public void DetermineIntronicEffect_IsStartSpliceSite()
        {
            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineIntronicEffect(_otherTranscriptRegions, new Interval(300, 303),
                VariantType.deletion);
            Assert.True(positionalEffect.IsStartSpliceSite);
        }

        [Fact]
        public void DetermineIntronicEffect_IsWithinFrameshiftIntron_NotInSpliceSite()
        {
            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineIntronicEffect(_otherTranscriptRegions, new Interval(701, 709),
                VariantType.deletion);
            Assert.True(positionalEffect.IsWithinFrameshiftIntron);
            Assert.False(positionalEffect.IsStartSpliceSite);
            Assert.False(positionalEffect.IsEndSpliceSite);
        }

        [Fact]
        public void DetermineIntronicEffect_IsWithinIntron()
        {
            IInterval variant          = new Interval(300, 302);
            var       positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineIntronicEffect(_otherTranscriptRegions, variant, VariantType.deletion);
            Assert.False(positionalEffect.IsWithinIntron);

            IInterval variant2          = new Interval(303, 303);
            var       positionalEffect2 = new TranscriptPositionalEffect();
            positionalEffect2.DetermineIntronicEffect(_otherTranscriptRegions, variant2, VariantType.deletion);
            Assert.True(positionalEffect2.IsWithinIntron);
        }

        [Fact]
        public void DetermineIntronicEffect_IsWithinSpliceSiteRegion()
        {
            var       positionalEffect = new TranscriptPositionalEffect();
            IInterval variant          = new Interval(298, 302);

            positionalEffect.DetermineIntronicEffect(_otherTranscriptRegions, variant, VariantType.deletion);
            Assert.True(positionalEffect.IsWithinSpliceSiteRegion);
        }

        [Fact]
        public void DetermineExonicEffect_HasExonOverlap()
        {
            IInterval variant  = new Interval(876686, 876686);
            var       position = new MappedPosition(349, 349, 349, 349, 349, 117, 117, 2, 2, 2, -1, -1, 2, 2);

            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineExonicEffect(_forwardTranscript, variant, position, 349, 349, 349, 349, "G",
                false);

            Assert.True(positionalEffect.HasExonOverlap);
        }

        [Fact]
        public void DetermineExonicEffect_AfterCoding()
        {
            IInterval variant  = new Interval(879600, 879600);
            var       position = new MappedPosition(1692, 1692, -1, -1, -1, -1, -1, -1, 7, 7, -1, -1, 12, 12);

            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineExonicEffect(_forwardTranscript, variant, position, 1692, 1692, -1, -1, "G",
                false);
            Assert.True(positionalEffect.AfterCoding);
        }

        [Fact]
        public void DetermineExonicEffect_WithinCdna()
        {
            IInterval variant  = new Interval(879600, 879600);
            var       position = new MappedPosition(1692, 1692, -1, -1, -1, -1, -1, -1, 7, 7, -1, -1, 12, 12);

            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineExonicEffect(_forwardTranscript, variant, position, 1692, 1692, -1, -1, "G",
                false);
            Assert.True(positionalEffect.WithinCdna);
        }

        [Fact]
        public void DetermineExonicEffect_WithinCds()
        {
            IInterval variant  = new Interval(876543, 876543);
            var       position = new MappedPosition(206, 206, 206, 206, 69, 69, 69, 69, 2, 2, -1, -1, 2, 2);

            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineExonicEffect(_forwardTranscript, variant, position, 206, 206, 206, 206, "G",
                false);
            Assert.True(positionalEffect.WithinCdna);
        }

        [Fact]
        public void DetermineExonicEffect_OverlapWithMicroRna()
        {
            IInterval variant  = new Interval(3477284, 3477284);
            var       position = new MappedPosition(71, 71, -1, -1, -1, -1, -1, -1, 1, 1, -1, -1, 0, 0);

            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineExonicEffect(_reverseTranscript, variant, position, 71, 71, -1, -1, "G", false);
            // TODO: enable this when we have a way to determine where the mature miRNA region is
            Assert.False(positionalEffect.OverlapWithMicroRna);
        }

        [Fact]
        public void ExonOverlaps_NoOverlap()
        {
            var transcriptRegions = new TranscriptRegion[]
            {
                new(100, 200, 300, 400, TranscriptRegionType.Exon, 1, null)
            };

            IInterval variant        = new Interval(201, 500);
            var       observedResult = transcriptRegions[0].Overlaps(variant);

            Assert.False(observedResult);
        }

        [Fact]
        public void IsMatureMirnaVariant_NullMirnas()
        {
            var observedResult = TranscriptPositionalEffect.IsMatureMirnaVariant(-1, -1, null, true);
            Assert.False(observedResult);
        }

        [Fact]
        public void IsWithinCds_ReturnFalse()
        {
            var positionalEffect = new TranscriptPositionalEffect();
            var observedResult   = positionalEffect.IsWithinCds(-1, -1, null, null);
            Assert.False(observedResult);
        }

        [Fact]
        public void IsWithinCds_ReturnTrue()
        {
            var positionalEffect = new TranscriptPositionalEffect();
            var observedResult   = positionalEffect.IsWithinCds(180, 180, null, null);
            Assert.True(observedResult);
        }

        [Fact]
        public void IsWithinCds_IsWithinFrameshiftIntron_OverlapCodingRegion_ReturnTrue()
        {
            var variant          = new Interval(100, 101);
            var codingRegion     = new Interval(90, 120);
            var positionalEffect = new TranscriptPositionalEffect {IsWithinFrameshiftIntron = true};

            var observedResult = positionalEffect.IsWithinCds(-1, -1, codingRegion, variant);
            Assert.True(observedResult);
        }

        [Fact]
        public void IsWithinCds_IsWithinFrameshiftIntron_ReturnFalse()
        {
            var variant          = new Interval(100, 101);
            var codingRegion     = new Interval(102, 120);
            var positionalEffect = new TranscriptPositionalEffect {IsWithinFrameshiftIntron = true};

            var observedResult = positionalEffect.IsWithinCds(-1, -1, codingRegion, variant);
            Assert.False(observedResult);
        }

        [Fact]
        public void IsAfterCoding_True_WhenInsertion()
        {
            var observedResult = TranscriptPositionalEffect.IsAfterCoding(101, 100, 100, 100);
            Assert.True(observedResult);
        }

        [Fact]
        public void IsBeforeCoding_True_WhenInsertion()
        {
            var observedResult = TranscriptPositionalEffect.IsBeforeCoding(101, 100, 100, 101);
            Assert.True(observedResult);
        }

        [Theory]
        [InlineData(100, 200, 300, true)]
        [InlineData(500, 600, 300, false)]
        public void IsWithinCdna(int cdnaStart, int cdnaEnd, int totalExonLen, bool expectedResult)
        {
            var observedResult = TranscriptPositionalEffect.IsWithinCdna(cdnaStart, cdnaEnd, totalExonLen);
            Assert.Equal(expectedResult, observedResult);
        }
    }
}