using Cache.Data;
using Moq;
using UnitTests.VariantAnnotation.Utilities;
using VariantAnnotation.AnnotatedPositions;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class VariantEffectTests
    {
        [Theory]
        [InlineData(false, false, false, false)]
        [InlineData(false, false, true, true)]
        [InlineData(true, false, true, false)]
        [InlineData(true, true, true, true)]
        public void IsSpliceAcceptorVariant(bool onReverseStrand, bool isStartSpliceSite, bool isEndSpliceSite,
            bool expectedResult)
        {
            var positionalEffect = new TranscriptPositionalEffect
            {
                IsStartSpliceSite = isStartSpliceSite,
                IsEndSpliceSite   = isEndSpliceSite
            };

            var variant = new Mock<ISimpleVariant>();
            variant.SetupGet(x => x.AltAllele).Returns("G");
            variant.SetupGet(x => x.RefAllele).Returns("C");

            var transcript = TranscriptMocker.GetTranscript(onReverseStrand, null, null, 100, 200);

            var variantEffect = new VariantEffect(positionalEffect, variant.Object, transcript, "", "", "", "",
                null, "", "");

            Assert.Equal(expectedResult, variantEffect.IsSpliceAcceptorVariant());
        }

        [Theory]
        [InlineData(false, false, false, false)]
        [InlineData(false, true, true, true)]
        [InlineData(true, false, false, false)]
        [InlineData(true, true, true, true)]
        public void IsSpliceDonorVariant(bool onReverseStrand, bool isStartSpliceSite, bool isEndSpliceSite,
            bool expectedResult)
        {
            var positionalEffect = new TranscriptPositionalEffect
            {
                IsStartSpliceSite = isStartSpliceSite,
                IsEndSpliceSite   = isEndSpliceSite
            };

            var variant = new Mock<ISimpleVariant>();
            variant.SetupGet(x => x.AltAllele).Returns("G");
            variant.SetupGet(x => x.RefAllele).Returns("C");

            var transcript = TranscriptMocker.GetTranscript(onReverseStrand, null, null, 100, 200);

            var variantEffect = new VariantEffect(positionalEffect, variant.Object, transcript, "", "", "", "",
                null, "", "");

            Assert.Equal(expectedResult, variantEffect.IsSpliceDonorVariant());
        }

        [Theory]
        [InlineData(1, "M", "KM", "", "TCT", true)]
        [InlineData(2, "M", "Mk", "", "TCT", false)]
        [InlineData(1, "K", "MK", "", "ATG", true)]
        public void IsStartRetainedVariant(int proteinBegin, string refAminoAcids, string altAminoAcids,
            string refAllele, string altAllele, bool isStartRetained)
        {
            var variant = new Mock<ISimpleVariant>();
            variant.SetupGet(x => x.AltAllele).Returns(refAllele);
            variant.SetupGet(x => x.RefAllele).Returns(altAllele);

            var transcript = TranscriptMocker.GetTranscript(false, null, null, 100, 200);

            var variantEffect = new VariantEffect(null, variant.Object, transcript, refAminoAcids, altAminoAcids, "",
                "",
                proteinBegin, refAminoAcids, altAminoAcids);

            if (isStartRetained) Assert.True(variantEffect.IsStartRetained());
            else Assert.False(variantEffect.IsStartRetained());
        }

        [Theory]
        [InlineData(false, true, false, false, false)]
        [InlineData(false, true, true, true, true)]
        [InlineData(false, false, true, true, false)]
        [InlineData(true, true, false, false, false)]
        [InlineData(true, true, true, true, true)]
        [InlineData(true, false, true, true, false)]
        public void IsFivePrimeUtrVariant(bool onReverseStrand, bool withinCdna, bool beforeCoding, bool afterCoding,
            bool expectedResult)
        {
            var positionalEffect = new TranscriptPositionalEffect
            {
                BeforeCoding = beforeCoding,
                AfterCoding  = afterCoding,
                WithinCdna   = withinCdna
            };

            var variant = new Mock<ISimpleVariant>();

            variant.SetupGet(x => x.AltAllele).Returns("G");
            variant.SetupGet(x => x.RefAllele).Returns("C");

            var codingRegion = new CodingRegion(100, 200, 1, 2, string.Empty, string.Empty, 0, 0, 0, null, null);
            var transcript   = TranscriptMocker.GetTranscript(onReverseStrand, null, codingRegion, 100, 200);

            var variantEffect = new VariantEffect(positionalEffect, variant.Object, transcript, "", "", "", "",
                null, "", "");

            Assert.Equal(expectedResult, variantEffect.IsFivePrimeUtrVariant());
        }

        [Fact]
        public void IsStopLost_DeletionOverStopCodon_ReturnTrue()
        {
            var positionalEffect = new TranscriptPositionalEffect
            {
                BeforeCoding = false,
                AfterCoding  = true,
                WithinCdna   = true
            };

            var variant = new Mock<ISimpleVariant>();
            variant.SetupGet(x => x.AltAllele).Returns("ATAGCCC");
            variant.SetupGet(x => x.RefAllele).Returns("A");

            var variantEffect = new VariantEffect(positionalEffect, variant.Object, null, "", "", "", "",
                null, "*", "X");

            Assert.True(variantEffect.IsStopLost());
        }
    }
}