using Intervals;
using VariantAnnotation.AnnotatedPositions.Transcript;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class FeatureVariantEffectsTests
    {
        [Theory]
        [InlineData(VariantType.deletion,         OverlapType.CompletelyOverlaps, EndpointOverlapType.Both,  true)]
        [InlineData(VariantType.copy_number_loss, OverlapType.CompletelyOverlaps, EndpointOverlapType.Both,  true)]
        [InlineData(VariantType.deletion,         OverlapType.Partial,            EndpointOverlapType.Start, false)]
        [InlineData(VariantType.copy_number_loss, OverlapType.Partial,            EndpointOverlapType.End,   false)]
        [InlineData(VariantType.copy_number_loss, OverlapType.CompletelyWithin,   EndpointOverlapType.None,  false)]
        public void Ablation(VariantType variantType, OverlapType overlapType, EndpointOverlapType endpointOverlapType, bool expectResult)
        {
            var  featureEffect  = new FeatureVariantEffects(overlapType, endpointOverlapType, false, variantType, true);
            bool observedResult = featureEffect.Ablation();
            Assert.Equal(expectResult, observedResult);
        }

        [Theory]
        [InlineData(VariantType.copy_number_gain,   OverlapType.CompletelyOverlaps, EndpointOverlapType.Both,  true)]
        [InlineData(VariantType.duplication,        OverlapType.CompletelyOverlaps, EndpointOverlapType.Both,  true)]
        [InlineData(VariantType.tandem_duplication, OverlapType.CompletelyOverlaps, EndpointOverlapType.Both,  true)]
        [InlineData(VariantType.copy_number_gain,   OverlapType.Partial,            EndpointOverlapType.Start, false)]
        [InlineData(VariantType.duplication,        OverlapType.CompletelyWithin,   EndpointOverlapType.None,  false)]
        [InlineData(VariantType.tandem_duplication, OverlapType.Partial,            EndpointOverlapType.End,   false)]
        public void Amplification(VariantType variantType, OverlapType overlapType, EndpointOverlapType endpointOverlapType, bool expectedResult)
        {
            var  featureEffect  = new FeatureVariantEffects(overlapType, endpointOverlapType, false, variantType, true);
            bool observedResult = featureEffect.Amplification();
            Assert.Equal(expectedResult, observedResult);
        }

        [Theory]
        [InlineData(VariantType.deletion,         OverlapType.Partial,            EndpointOverlapType.Start, true)]
        [InlineData(VariantType.copy_number_loss, OverlapType.Partial,            EndpointOverlapType.End,   true)]
        [InlineData(VariantType.copy_number_loss, OverlapType.CompletelyWithin,   EndpointOverlapType.None,  true)]
        [InlineData(VariantType.deletion,         OverlapType.CompletelyOverlaps, EndpointOverlapType.Both,  false)]
        [InlineData(VariantType.copy_number_loss, OverlapType.CompletelyOverlaps, EndpointOverlapType.Both,  false)]
        public void Truncation(VariantType variantType, OverlapType overlapType, EndpointOverlapType endpointOverlapType, bool expectedResult)
        {
            var  featureEffect  = new FeatureVariantEffects(overlapType, endpointOverlapType, false, variantType, true);
            bool observedResult = featureEffect.Truncation();
            Assert.Equal(expectedResult, observedResult);
        }

        [Theory]
        [InlineData(VariantType.copy_number_gain,   OverlapType.CompletelyWithin,   EndpointOverlapType.None,  true)]
        [InlineData(VariantType.duplication,        OverlapType.CompletelyWithin,   EndpointOverlapType.None,  true)]
        [InlineData(VariantType.tandem_duplication, OverlapType.CompletelyWithin,   EndpointOverlapType.None,  true)]
        [InlineData(VariantType.insertion,          OverlapType.CompletelyWithin,   EndpointOverlapType.None,  true)]
        [InlineData(VariantType.copy_number_gain,   OverlapType.CompletelyOverlaps, EndpointOverlapType.Both,  false)]
        [InlineData(VariantType.duplication,        OverlapType.Partial,            EndpointOverlapType.Start, false)]
        [InlineData(VariantType.tandem_duplication, OverlapType.Partial,            EndpointOverlapType.End,   false)]
        public void Elongation(VariantType variantType, OverlapType overlapType, EndpointOverlapType endpointOverlapType, bool expectedResult)
        {
            var  featureEffect  = new FeatureVariantEffects(overlapType, endpointOverlapType, false, variantType, true);
            bool observedResult = featureEffect.Elongation();
            Assert.Equal(expectedResult, observedResult);
        }

        [Theory]
        [InlineData(VariantType.copy_number_gain,   OverlapType.Partial,            EndpointOverlapType.End,   false, false)]
        [InlineData(VariantType.copy_number_gain,   OverlapType.Partial,            EndpointOverlapType.End,   true,  true)]
        [InlineData(VariantType.copy_number_gain,   OverlapType.CompletelyWithin,   EndpointOverlapType.None,  false, false)]
        [InlineData(VariantType.copy_number_gain,   OverlapType.Partial,            EndpointOverlapType.Start, false, true)]
        [InlineData(VariantType.copy_number_gain,   OverlapType.Partial,            EndpointOverlapType.Start, true,  false)]
        [InlineData(VariantType.copy_number_gain,   OverlapType.CompletelyOverlaps, EndpointOverlapType.Both,  false, false)]
        [InlineData(VariantType.duplication,        OverlapType.Partial,            EndpointOverlapType.End,   true,  true)]
        [InlineData(VariantType.tandem_duplication, OverlapType.Partial,            EndpointOverlapType.End,   true,  true)]
        [InlineData(VariantType.duplication,        OverlapType.Partial,            EndpointOverlapType.Start, true,  false)]
        [InlineData(VariantType.tandem_duplication, OverlapType.Partial,            EndpointOverlapType.Start, true,  false)]
        [InlineData(VariantType.duplication,        OverlapType.CompletelyWithin,   EndpointOverlapType.Start, false, true)]
        [InlineData(VariantType.duplication,        OverlapType.CompletelyWithin,   EndpointOverlapType.End,   true,  true)]
        public void FivePrimeDuplicatedTranscript(VariantType variantType,     OverlapType overlapType, EndpointOverlapType endpointOverlapType,
                                                  bool        onReverseStrand, bool        expectedResult)
        {
            var  featureEffect  = new FeatureVariantEffects(overlapType, endpointOverlapType, onReverseStrand, variantType, true);
            bool observedResult = featureEffect.FivePrimeDuplicatedTranscript();
            Assert.Equal(expectedResult, observedResult);
        }

        [Theory]
        [InlineData(VariantType.copy_number_gain,   OverlapType.Partial,            EndpointOverlapType.End,   false, true)]
        [InlineData(VariantType.copy_number_gain,   OverlapType.Partial,            EndpointOverlapType.End,   true,  false)]
        [InlineData(VariantType.copy_number_gain,   OverlapType.CompletelyWithin,   EndpointOverlapType.None,  false, false)]
        [InlineData(VariantType.copy_number_gain,   OverlapType.Partial,            EndpointOverlapType.Start, false, false)]
        [InlineData(VariantType.copy_number_gain,   OverlapType.Partial,            EndpointOverlapType.Start, true,  true)]
        [InlineData(VariantType.copy_number_gain,   OverlapType.CompletelyOverlaps, EndpointOverlapType.Both,  false, false)]
        [InlineData(VariantType.duplication,        OverlapType.Partial,            EndpointOverlapType.End,   true,  false)]
        [InlineData(VariantType.tandem_duplication, OverlapType.Partial,            EndpointOverlapType.End,   true,  false)]
        [InlineData(VariantType.duplication,        OverlapType.Partial,            EndpointOverlapType.Start, true,  true)]
        [InlineData(VariantType.tandem_duplication, OverlapType.Partial,            EndpointOverlapType.Start, true,  true)]
        [InlineData(VariantType.duplication,        OverlapType.CompletelyWithin,   EndpointOverlapType.End,   false, true)]
        [InlineData(VariantType.duplication,        OverlapType.CompletelyWithin,   EndpointOverlapType.Start, true,  true)]
        public void ThreePrimeDuplicatedTranscript(VariantType variantType,     OverlapType overlapType, EndpointOverlapType endpointOverlapType,
                                                   bool        onReverseStrand, bool        expectedResult)
        {
            var  featureEffect  = new FeatureVariantEffects(overlapType, endpointOverlapType, onReverseStrand, variantType, true);
            bool observedResult = featureEffect.ThreePrimeDuplicatedTranscript();
            Assert.Equal(expectedResult, observedResult);
        }
    }
}