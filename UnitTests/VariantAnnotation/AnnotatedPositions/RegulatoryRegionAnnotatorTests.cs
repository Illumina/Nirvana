using System.Linq;
using Genome;
using Moq;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class RegulatoryRegionAnnotatorTests
    {
        private readonly IChromosome _chromosome;

        public RegulatoryRegionAnnotatorTests()
        {
            _chromosome = new Chromosome("chrBob", "bob", 0);
        }

        [Fact]
        public void Annotate_Promoter()
        {
            var variant          = GetVariant();
            var regulatoryRegion = GetRegulatoryRegion();

            const ConsequenceTag expectedConsequence = ConsequenceTag.regulatory_region_variant;
            var annotatedRegulatoryRegion = RegulatoryRegionAnnotator.Annotate(variant, regulatoryRegion);
            var consequences = annotatedRegulatoryRegion.Consequences.ToList();

            Assert.NotNull(annotatedRegulatoryRegion);
            Assert.Single(consequences);
            Assert.Equal(expectedConsequence, consequences[0]);
        }

        private IRegulatoryRegion GetRegulatoryRegion()
        {
            return new RegulatoryRegion(_chromosome, 948000, 950401, CompactId.Convert("ENSR00001037666"),
                RegulatoryRegionType.promoter);
        }

        private static IVariant GetVariant()
        {
            var behavior = new AnnotationBehavior(false, false, false, false, false);

            var variant = new Mock<IVariant>();
            variant.SetupGet(x => x.Type).Returns(VariantType.SNV);
            variant.SetupGet(x => x.Start).Returns(949523);
            variant.SetupGet(x => x.End).Returns(949523);
            variant.SetupGet(x => x.Behavior).Returns(behavior);
            return variant.Object;
        }
    }
}
