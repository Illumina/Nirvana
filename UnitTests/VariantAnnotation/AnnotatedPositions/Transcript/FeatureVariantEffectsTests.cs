using Intervals;
using VariantAnnotation.AnnotatedPositions.Transcript;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class FeatureVariantEffectsTests
    {
        [Theory]
        [InlineData(VariantType.deletion, true)]
        [InlineData(VariantType.copy_number_loss, true)]
        public void Ablation(VariantType type, bool expectResult)
        {
            var featureEffect = new FeatureVariantEffects(new Interval(100, 200), type, new Interval(50, 300), true);
            Assert.Equal(expectResult, featureEffect.Ablation());
        }

        [Theory]
        [InlineData(100, 300, true)]
        [InlineData(200, 300, false)]
        [InlineData(180, 200, false)]
        public void Not_Ablation_if_not_completely_overlapped(int variantStart, int variantEnd, bool expectedResult)
        {
            var featureEffect = new FeatureVariantEffects(new Interval(150, 250), VariantType.deletion, new Interval(variantStart, variantEnd), false);
            Assert.Equal(expectedResult, featureEffect.Ablation());
        }

        [Theory]
        [InlineData(VariantType.deletion, 100, 300, false)]
        [InlineData(VariantType.copy_number_gain, 100, 300, true)]
        [InlineData(VariantType.copy_number_gain, 100, 200, false)]
        public void Amplification(VariantType variantType, int variantStart, int variantEnd, bool expectedResult)
        {
            var featureEffect = new FeatureVariantEffects(new Interval(150, 250), variantType, new Interval(variantStart, variantEnd), true);
            Assert.Equal(expectedResult, featureEffect.Amplification());
        }

        [Theory]
        [InlineData(VariantType.deletion, 100, 300, false)]
        [InlineData(VariantType.deletion, 100, 200, true)]
        [InlineData(VariantType.copy_number_gain, 100, 200, false)]
        public void Truncation(VariantType variantType, int variantStart, int variantEnd, bool expectedResult)
        {
            var featureEffect = new FeatureVariantEffects(new Interval(150, 250), variantType, new Interval(variantStart, variantEnd), true);
            Assert.Equal(expectedResult, featureEffect.Truncation());
        }

        [Theory]
        [InlineData(VariantType.deletion, 100, 300, false)]
        [InlineData(VariantType.copy_number_gain, 180, 200, true)]
        [InlineData(VariantType.copy_number_gain, 100, 200, false)]
        [InlineData(VariantType.insertion, 201, 200, true)]
        public void Elongation(VariantType variantType, int variantStart, int variantEnd, bool expectedResult)
        {
            var featureEffect = new FeatureVariantEffects(new Interval(150, 250), variantType, new Interval(variantStart, variantEnd), true);
            Assert.Equal(expectedResult, featureEffect.Elongation());
        }
    }
}