using Genome;
using Moq;
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.Vcf
{
    public sealed class ReferenceVariantCreatorTests
    {
        [Fact]
        public void ReferenceVariant_have_annotationBehaviorNull()
        {
            var chrom = new Mock<IChromosome>();
            chrom.Setup(x => x.EnsemblName).Returns("1");
	        var variant = ReferenceVariantCreator.Create(chrom.Object, 100, 101, "A", ".", null);
            Assert.False(variant.IsRefMinor);
            Assert.Null(variant.Behavior);
        }

        [Fact]
        public void RefMinorSite_have_correct_behavior()
        {
            var chrom = new Mock<IChromosome>();
            chrom.Setup(x => x.EnsemblName).Returns("1");
            var variant = ReferenceVariantCreator.Create(chrom.Object, 100, 100, "A", ".", "T");
			Assert.True(variant.IsRefMinor);
            Assert.NotNull(variant.Behavior);
            Assert.True(variant.Behavior.NeedFlankingTranscript);
            Assert.True(variant.Behavior.NeedSaPosition);
            Assert.False(variant.Behavior.NeedSaInterval);
            Assert.False(variant.Behavior.ReducedTranscriptAnnotation);
            Assert.False(variant.Behavior.StructuralVariantConsequence);

	        var variant2 = ReferenceVariantCreator.Create(chrom.Object, 101, 101, "A", ".", null);
			Assert.False(variant2.IsRefMinor);
            Assert.Null(variant2.Behavior);

	        var variant3 = ReferenceVariantCreator.Create(chrom.Object, 100, 110, "A", ".", null);
			Assert.False(variant3.IsRefMinor);
            Assert.Null(variant3.Behavior);
        }
    }
}