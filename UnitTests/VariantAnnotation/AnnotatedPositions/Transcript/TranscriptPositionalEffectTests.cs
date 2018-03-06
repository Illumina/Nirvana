using Moq;
using VariantAnnotation.Algorithms;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class TranscriptPositionalEffectTests
    {
        private readonly Mock<ITranscript> _forwardTranscript; // use info from "ENST00000455979.1" with modification
        private readonly Mock<ITranscript> _reverseTranscript; // use info from "ENST00000385042"
        private readonly ITranscriptRegion[] _forwardTranscriptRegions;
        private readonly ITranscriptRegion[] _otherTranscriptRegions;

        public TranscriptPositionalEffectTests()
        {
            var chromosome  = new Chromosome("chr1", "1", 0);
            const int start = 874655;
            const int end   = 879639;

            _otherTranscriptRegions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 200, 300, 1, 186),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 301, 400, 186, 187),
                new TranscriptRegion(TranscriptRegionType.Exon, 2, 401, 699, 187, 349),
                new TranscriptRegion(TranscriptRegionType.Intron, 2, 700, 709, 359, 360),
                new TranscriptRegion(TranscriptRegionType.Exon, 3, 710, 800, 350, 465)
            };

            _forwardTranscriptRegions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 874655, 874840, 1, 186),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 874841, 876523, 186, 187),
                new TranscriptRegion(TranscriptRegionType.Exon, 2, 876524, 876686, 187, 349),
                new TranscriptRegion(TranscriptRegionType.Intron, 2, 876687, 877515, 349, 350),
                new TranscriptRegion(TranscriptRegionType.Exon, 3, 877516, 877631, 350, 465),
                new TranscriptRegion(TranscriptRegionType.Intron, 3, 877632, 877789, 465, 466),
                new TranscriptRegion(TranscriptRegionType.Exon, 4, 877790, 877868, 466, 544),
                new TranscriptRegion(TranscriptRegionType.Intron, 4, 877869, 877938, 544, 545),
                new TranscriptRegion(TranscriptRegionType.Exon, 5, 877939, 878438, 545, 1044),
                new TranscriptRegion(TranscriptRegionType.Intron, 5, 878439, 878632, 1044, 1045),
                new TranscriptRegion(TranscriptRegionType.Exon, 6, 878633, 878757, 1045, 1169),
                new TranscriptRegion(TranscriptRegionType.Intron, 6, 878758, 879077, 1169, 1170),
                new TranscriptRegion(TranscriptRegionType.Exon, 7, 879078, 879639, 1170, 1731)
            };

            var reverseTranscriptRegions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 3477259, 3477354, 1, 96)
            };

            var translation = new Mock<ITranslation>();
            translation.SetupGet(x => x.CodingRegion).Returns(new CodingRegion(874655, 879533, 1, 1625, 1625));

            var gene = new Mock<IGene>();
            gene.SetupGet(x => x.OnReverseStrand).Returns(false);

            _forwardTranscript = new Mock<ITranscript>();
            _forwardTranscript.SetupGet(x => x.Chromosome).Returns(chromosome);
            _forwardTranscript.SetupGet(x => x.Start).Returns(start);
            _forwardTranscript.SetupGet(x => x.End).Returns(end);
            _forwardTranscript.SetupGet(x => x.Gene).Returns(gene.Object);
            _forwardTranscript.SetupGet(x => x.TranscriptRegions).Returns(_forwardTranscriptRegions);
            _forwardTranscript.SetupGet(x => x.Translation).Returns(translation.Object);
            _forwardTranscript.SetupGet(x => x.TotalExonLength).Returns(1731);

            _reverseTranscript = new Mock<ITranscript>();
            _reverseTranscript.SetupGet(x => x.Chromosome).Returns(chromosome);
            _reverseTranscript.SetupGet(x => x.Start).Returns(3477259);
            _reverseTranscript.SetupGet(x => x.Start).Returns(3477354);
            _reverseTranscript.SetupGet(x => x.Gene.OnReverseStrand).Returns(true);
            _reverseTranscript.SetupGet(x => x.Translation).Returns((ITranslation)null);
            _reverseTranscript.SetupGet(x => x.BioType).Returns(BioType.miRNA);
            _reverseTranscript.SetupGet(x => x.TranscriptRegions).Returns(reverseTranscriptRegions);
            _reverseTranscript.SetupGet(x => x.MicroRnas).Returns(new IInterval[] { new Interval(61, 81) });
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
            var transcriptRegions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 201342300, 201342340, 1, 186),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 201342340, 201342343, 186, 187),
                new TranscriptRegion(TranscriptRegionType.Exon, 2, 201342344, 201342400, 187, 349)
            };

            IInterval variant    = new Interval(201342344, 201342344);
            var positionalEffect = new TranscriptPositionalEffect();
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
            positionalEffect.DetermineIntronicEffect(_otherTranscriptRegions, new Interval(300, 303), VariantType.deletion);
            Assert.True(positionalEffect.IsStartSpliceSite);
        }

        [Fact]
        public void DetermineIntronicEffect_IsWithinFrameshiftIntron()
        {
            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineIntronicEffect(_otherTranscriptRegions, new Interval(702, 705), VariantType.deletion);
            Assert.True(positionalEffect.IsWithinFrameshiftIntron);
        }

        [Fact]
        public void DetermineIntronicEffect_IsWithinIntron()
        {
            IInterval variant    = new Interval(300, 302);
            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineIntronicEffect(_otherTranscriptRegions, variant, VariantType.deletion);
            Assert.False(positionalEffect.IsWithinIntron);

            IInterval variant2    = new Interval(303, 303);
            var positionalEffect2 = new TranscriptPositionalEffect();
            positionalEffect2.DetermineIntronicEffect(_otherTranscriptRegions, variant2, VariantType.deletion);
            Assert.True(positionalEffect2.IsWithinIntron);
        }

        [Fact]
        public void DetermineIntronicEffect_IsWithinSpliceSiteRegion()
        {
            var positionalEffect = new TranscriptPositionalEffect();
            IInterval variant    = new Interval(298, 302);

            positionalEffect.DetermineIntronicEffect(_otherTranscriptRegions, variant, VariantType.deletion);
            Assert.True(positionalEffect.IsWithinSpliceSiteRegion);
        }

        [Fact]
        public void DetermineExonicEffect_HasExonOverlap()
        {
            IInterval variant = new Interval(876686, 876686);
            var position      = new MappedPosition(349, 349, 349, 349, 117, 117, 2, 2, -1, -1, 2, 2);

            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineExonicEffect(_forwardTranscript.Object, variant, position, 349, 349, 349, 349, "G", false);

            Assert.True(positionalEffect.HasExonOverlap);
        }

        [Fact]
        public void DetermineExonicEffect_AfterCoding()
        {
            IInterval variant = new Interval(879600, 879600);
            var position      = new MappedPosition(1692, 1692, -1, -1, -1, -1, 7, 7, -1, -1, 12, 12);

            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineExonicEffect(_forwardTranscript.Object, variant, position, 1692, 1692, -1, -1, "G", false);
            Assert.True(positionalEffect.AfterCoding);
        }

        [Fact]
        public void DetermineExonicEffect_WithinCdna()
        {
            IInterval variant = new Interval(879600, 879600);
            var position      = new MappedPosition(1692, 1692, -1, -1, -1, -1, 7, 7, -1, -1, 12, 12);

            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineExonicEffect(_forwardTranscript.Object, variant, position, 1692, 1692, -1, -1, "G", false);
            Assert.True(positionalEffect.WithinCdna);
        }

        [Fact]
        public void DetermineExonicEffect_WithinCds()
        {
            IInterval variant = new Interval(876543, 876543);
            var position      = new MappedPosition(206, 206, 206, 206, 69, 69, 2, 2, -1, -1, 2, 2);

            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineExonicEffect(_forwardTranscript.Object, variant, position, 206, 206, 206, 206, "G", false);
            Assert.True(positionalEffect.WithinCdna);
        }

        [Fact]
        public void DetermineExonicEffect_OverlapWithMicroRna()
        {
            IInterval variant = new Interval(3477284, 3477284);
            var position      = new MappedPosition(71, 71, -1, -1, -1, -1, 1, 1, -1, -1, 0, 0);

            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineExonicEffect(_reverseTranscript.Object, variant, position, 71, 71, -1, -1, "G", false);
            Assert.True(positionalEffect.OverlapWithMicroRna);
        }

        [Fact]
        public void ExonOverlaps_NoOverlap()
        {
            var transcriptRegions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 100, 200, 300, 400)
            };

            IInterval variant  = new Interval(201, 500);
            var observedResult = transcriptRegions[0].Overlaps(variant);

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
            var observedResult = positionalEffect.IsWithinCds(-1, -1, null, null);
            Assert.False(observedResult);
        }

        [Fact]
        public void IsWithinCds_ReturnTrue()
        {
            var positionalEffect = new TranscriptPositionalEffect();
            var observedResult = positionalEffect.IsWithinCds(180, 180, null, null);
            Assert.True(observedResult);
        }

        [Fact]
        public void IsWithinCds_IsWithinFrameshiftIntron_OverlapCodingRegion_ReturnTrue()
        {
            var variant          = new Interval(100, 101);
            var codingRegion     = new Interval(90, 120);
            var positionalEffect = new TranscriptPositionalEffect { IsWithinFrameshiftIntron = true };

            var observedResult = positionalEffect.IsWithinCds(-1, -1, codingRegion, variant);
            Assert.True(observedResult);
        }

        [Fact]
        public void IsWithinCds_IsWithinFrameshiftIntron_ReturnFalse()
        {
            var variant          = new Interval(100, 101);
            var codingRegion     = new Interval(102, 120);
            var positionalEffect = new TranscriptPositionalEffect { IsWithinFrameshiftIntron = true };

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