using Variants;
using Xunit;

namespace UnitTests.Variants
{
    public sealed class AnnotationBehaviorTests
    {
        [Fact]
        public void AnnotationBehavior_Set()
        {
            var behavior = new AnnotationBehavior(true, false, true, false, true, false, true);
            Assert.False(behavior.NeedFlankingTranscript);
            Assert.False(behavior.NeedSaInterval);
            Assert.True(behavior.NeedSaPosition);
            Assert.True(behavior.NeedVerboseTranscripts);
            Assert.True(behavior.ReducedTranscriptAnnotation);
            Assert.True(behavior.ReportOverlappingGenes);
            Assert.False(behavior.StructuralVariantConsequence);
        }
    }
}
