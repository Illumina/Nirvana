using Variants;
using Xunit;

namespace UnitTests.Variants
{
    public sealed class AnnotationBehaviorTests
    {
        [Fact]
        public void AnnotationBehavior_Set()
        {
            var behavior = new AnnotationBehavior(true, true, true, true, true);
            Assert.True(behavior.NeedFlankingTranscript);
            Assert.True(behavior.NeedSaInterval);
            Assert.True(behavior.NeedSaPosition);
            Assert.True(behavior.ReducedTranscriptAnnotation);
            Assert.True(behavior.StructuralVariantConsequence);
        }
    }
}
