using Moq;
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
        private readonly IInterval[] _introns = {
            new Interval(301, 400),
            new Interval(700, 709)
        };

        private readonly Mock<ITranscript> _forwardTranscript; //use info from "ENST00000455979.1" with modification
        private readonly Mock<ITranscript> _reverseTranscript; //use info from "ENST00000385042"
        private readonly ICdnaCoordinateMap[] _cdnaMaps;
        private readonly ITranslation _translation;

        public TranscriptPositionalEffectTests()
        {
            var chromosome = new Chromosome("chr1", "1", 0);
            var start      = 874655;
            var end        = 879639;

            var introns = new IInterval[]
            {
                new Interval(874841,876523),
                new Interval(876687,877515),
                new Interval(877632,877789),
                new Interval(877869,877938),
                new Interval(878439,878632),
                new Interval(878758,879077)
            };

            _cdnaMaps = new ICdnaCoordinateMap[]
            {
                new CdnaCoordinateMap(874655,874840,1,186),
                new CdnaCoordinateMap(876524,876686,187,349),
                new CdnaCoordinateMap(877516,877631,350,465),
                new CdnaCoordinateMap(877790,877868,466,544),
                new CdnaCoordinateMap(877939,878438,545,1044),
                new CdnaCoordinateMap(878633,878757,1045,1169),
                new CdnaCoordinateMap(879078,879639,1170,1731)
            };

            var translation = new Mock<ITranslation>();
            translation.SetupGet(x => x.CodingRegion).Returns(new CdnaCoordinateMap(874655, 879533, 1, 1625));
            _translation = translation.Object;

            var gene = new Mock<IGene>();
            gene.SetupGet(x => x.OnReverseStrand).Returns(false);

            _forwardTranscript = new Mock<ITranscript>();
            _forwardTranscript.SetupGet(x => x.Chromosome).Returns(chromosome);
            _forwardTranscript.SetupGet(x => x.Start).Returns(start);
            _forwardTranscript.SetupGet(x => x.End).Returns(end);
            _forwardTranscript.SetupGet(x => x.Gene).Returns(gene.Object);
            _forwardTranscript.SetupGet(x => x.Introns).Returns(introns);
            _forwardTranscript.SetupGet(x => x.CdnaMaps).Returns(_cdnaMaps);
            _forwardTranscript.SetupGet(x => x.Translation).Returns(translation.Object);
            _forwardTranscript.SetupGet(x => x.TotalExonLength).Returns(1731);

            _reverseTranscript = new Mock<ITranscript>();
            _reverseTranscript.SetupGet(x => x.Chromosome).Returns(chromosome);
            _reverseTranscript.SetupGet(x => x.Start).Returns(3477259);
            _reverseTranscript.SetupGet(x => x.Start).Returns(3477354);
            _reverseTranscript.SetupGet(x => x.Gene.OnReverseStrand).Returns(true);
            _reverseTranscript.SetupGet(x => x.Translation).Returns((ITranslation)null);
            _reverseTranscript.SetupGet(x => x.BioType).Returns(BioType.miRNA);
            _reverseTranscript.SetupGet(x => x.CdnaMaps).Returns(new ICdnaCoordinateMap[] { new CdnaCoordinateMap(3477259, 3477354, 1, 96) });
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
            var introns = new IInterval[1];
            introns[0] = new Interval(201342340, 201342343);

            var variant = new Mock<IVariant>();
            variant.SetupGet(x => x.Start).Returns(201342344);
            variant.SetupGet(x => x.End).Returns(201342344);

            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineIntronicEffect(introns, variant.Object, VariantType.SNV);

            Assert.True(positionalEffect.IsWithinSpliceSiteRegion);
        }

        [Fact]
        public void DetermineIntronicEffect_IsEndSpliceSite()
        {
            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineIntronicEffect(_introns, new Interval(400, 400), VariantType.SNV);
            Assert.True(positionalEffect.IsEndSpliceSite);
        }

        [Fact]
        public void DetermineIntronicEffect_IsStartSpliceSite()
        {
            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineIntronicEffect(_introns, new Interval(300, 303), VariantType.deletion);
            Assert.True(positionalEffect.IsStartSpliceSite);
        }

        [Fact]
        public void DetermineIntronicEffect_IsWithinFrameshiftIntron()
        {
            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineIntronicEffect(_introns, new Interval(702, 705), VariantType.deletion);
            Assert.True(positionalEffect.IsWithinFrameshiftIntron);
        }

        [Fact]
        public void DetermineIntronicEffect_IsWithinIntron()
        {
            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineIntronicEffect(_introns, new Interval(300, 302), VariantType.deletion);
            Assert.False(positionalEffect.IsWithinIntron);

            var positionalEffect2 = new TranscriptPositionalEffect();
            positionalEffect2.DetermineIntronicEffect(_introns, new Interval(303, 303), VariantType.deletion);
            Assert.True(positionalEffect2.IsWithinIntron);

        }

        [Fact]
        public void DetermineIntronicEffect_IsWithinSpliceSiteRegion()
        {
            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineIntronicEffect(_introns, new Interval(298, 302), VariantType.deletion);
            Assert.True(positionalEffect.IsWithinSpliceSiteRegion);
        }

        [Fact]
        public void DetermineExonicEffect_HasExonOverlap()
        {
            var variantStart = 876686;
            var variantEnd = 876686;
            var mappedPosition = MappedPositionsUtils.ComputeMappedPositions(variantStart, variantEnd, _forwardTranscript.Object);
            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineExonicEffect(_forwardTranscript.Object, new Interval(variantStart, variantEnd), mappedPosition, "G", false);
            Assert.True(positionalEffect.HasExonOverlap);

        }



        [Fact]
        public void DetermineExonicEffect_AfterCoding()
        {
            var variantStart = 879600;
            var variantEnd = 879600;
            var mappedPosition = MappedPositionsUtils.ComputeMappedPositions(variantStart, variantEnd, _forwardTranscript.Object);
            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineExonicEffect(_forwardTranscript.Object, new Interval(variantStart, variantEnd), mappedPosition, "G", false);
            Assert.True(positionalEffect.AfterCoding);
        }

        [Fact]
        public void DetermineExonicEffect_WithinCdna()
        {
            var variantStart = 879600;
            var variantEnd = 879600;
            var mappedPosition = MappedPositionsUtils.ComputeMappedPositions(variantStart, variantEnd, _forwardTranscript.Object);
            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineExonicEffect(_forwardTranscript.Object, new Interval(variantStart, variantEnd), mappedPosition, "G", false);
            Assert.True(positionalEffect.WithinCdna);
        }
        [Fact]
        public void DetermineExonicEffect_WithinCds()
        {
            var variantStart = 876543;
            var variantEnd = 876543;
            var mappedPosition = MappedPositionsUtils.ComputeMappedPositions(variantStart, variantEnd, _forwardTranscript.Object);
            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineExonicEffect(_forwardTranscript.Object, new Interval(variantStart, variantEnd), mappedPosition, "G", false);
            Assert.True(positionalEffect.WithinCdna);
        }

        [Fact]
        public void DetermineExonicEffect_OverlapWithMicroRna()
        {
            var variantStart = 3477284;
            var variantEnd = 3477284;
            var mappedPosition = MappedPositionsUtils.ComputeMappedPositions(variantStart, variantEnd, _reverseTranscript.Object);
            var positionalEffect = new TranscriptPositionalEffect();
            positionalEffect.DetermineExonicEffect(_reverseTranscript.Object, new Interval(variantStart, variantEnd), mappedPosition, "G", false);
            Assert.True(positionalEffect.OverlapWithMicroRna);
        }

        [Fact]
        public void ExonOverlaps_NoOverlap()
        {
            var cdnaMaps = new ICdnaCoordinateMap[1];
            cdnaMaps[0]  = new CdnaCoordinateMap(100, 200, 300, 400);

            var variantInterval = new Interval(201, 500);
            var observedResult = TranscriptPositionalEffect.ExonOverlaps(cdnaMaps, variantInterval);

            Assert.False(observedResult);
        }

        [Fact]
        public void IsMatureMirnaVariant_NullMirnas()
        {
            var mappedPositions = MappedPositionsUtils.ComputeMappedPositions(100, 200, _forwardTranscript.Object);
            var observedResult  = TranscriptPositionalEffect.IsMatureMirnaVariant(mappedPositions, null, true);

            Assert.False(observedResult);
        }

        [Fact]
        public void IsWithinCds_NullTranslation()
        {
            var positionalEffect = new TranscriptPositionalEffect();
            var observedResult = positionalEffect.IsWithinCds(null, null, 0, 0);

            Assert.False(observedResult);
        }

        [Fact]
        public void IsWithinCds_WithinFrameshiftIntron()
        {
            var positionalEffect = new TranscriptPositionalEffect { IsWithinFrameshiftIntron = true };
            var observedResult = positionalEffect.IsWithinCds(_cdnaMaps, _translation, 877878, 877929);

            Assert.True(observedResult);
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
            var impactedCdnaInterval = new Interval(cdnaStart, cdnaEnd);
            var observedResult = TranscriptPositionalEffect.IsWithinCdna(impactedCdnaInterval, totalExonLen);
            Assert.Equal(expectedResult, observedResult);
        }
    }
}