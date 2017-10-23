using Moq;
using VariantAnnotation.GeneAnnotation;
using VariantAnnotation.Interface.GeneAnnotation;
using Xunit;

namespace UnitTests.VariantAnnotation.GeneAnnotation
{
    public sealed class GeneAnnotatorTests
    {
        [Fact]
        public void Annoate_noAnnotation()
        {
            var annotationProvider = new Mock<IGeneAnnotationProvider>();
            annotationProvider.Setup(x => x.Annotate(It.IsAny<string>())).Returns((IAnnotatedGene)null);

            var observedResult = GeneAnnotator.Annotate(new[] {"gene1", "gene2"}, annotationProvider.Object);

            Assert.Equal(0,observedResult.Count);
        }

        [Fact]
        public void Annoate()
        {
            var annotationProvider = new Mock<IGeneAnnotationProvider>();
            annotationProvider.Setup(x => x.Annotate("gene2")).Returns((IAnnotatedGene)null);
            var geneAnnotation = new Mock<IAnnotatedGene>();
            annotationProvider.Setup(x => x.Annotate("gene1")).Returns(geneAnnotation.Object);


            var observedResult = GeneAnnotator.Annotate(new[] { "gene1", "gene2" }, annotationProvider.Object);

            Assert.Equal(1, observedResult.Count);
            Assert.Equal(geneAnnotation.Object,observedResult[0]);

        }
    }
}