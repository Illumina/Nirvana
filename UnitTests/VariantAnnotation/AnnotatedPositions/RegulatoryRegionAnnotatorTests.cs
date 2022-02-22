using System.Linq;
using Cache.Data;
using Moq;
using UnitTests.TestUtilities;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class RegulatoryRegionAnnotatorTests
    {
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

        private RegulatoryRegion GetRegulatoryRegion()
        {
            return new RegulatoryRegion(ChromosomeUtilities.Chr1, 948000, 950401, "ENSR00001037666", BioType.promoter,
                null, null, null);
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
