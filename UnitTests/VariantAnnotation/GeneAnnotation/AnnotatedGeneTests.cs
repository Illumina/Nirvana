using CommonUtilities;
using Moq;
using VariantAnnotation.GeneAnnotation;
using VariantAnnotation.Interface.GeneAnnotation;
using Xunit;

namespace UnitTests.VariantAnnotation.GeneAnnotation
{
    public sealed class AnnotatedGeneTests
    {
        [Fact]
        public void SerializeJson()
        {
            var geneAnnotation1 = new Mock<IGeneAnnotationSource>();
            var geneAnnotation2 = new Mock<IGeneAnnotationSource>();
            var geneAnnotations = new [] {geneAnnotation1.Object, geneAnnotation2.Object};

            geneAnnotation1.SetupGet(x => x.DataSource).Returns("annotation1");
            geneAnnotation1.Setup(x => x.JsonStrings).Returns(new[]
            {
                "{\"mimNumber\":603024,\"description\":\"AT rich interactive domain 1A, SWI-like\",\"phenotypes\":[{\"mimNumber\":614607,\"phenotype\":\"Coffin-Siris syndrome 2\",\"mapping\":\"molecular basis of the disorder is known\",\"inheritances\":[\"Autosomal dominant\"]}]}",
                "{\"mimNumber\":300531,\"description\":\"Sprouty, Drosophila, homolog of, 3\"}"
            });
            geneAnnotation1.SetupGet(x => x.IsArray).Returns(true);

            geneAnnotation2.SetupGet(x => x.DataSource).Returns("annotation2");
            geneAnnotation2.Setup(x => x.JsonStrings).Returns(new[] {"0.154"});
            geneAnnotation2.SetupGet(x => x.IsArray).Returns(false);


            var annotatedGene = new AnnotatedGene("Gene1", geneAnnotations);

            const string expectedLine = "{\"name\":\"Gene1\",\"annotation1\":[{\"mimNumber\":603024,\"description\":\"AT rich interactive domain 1A, SWI-like\",\"phenotypes\":[{\"mimNumber\":614607,\"phenotype\":\"Coffin-Siris syndrome 2\",\"mapping\":\"molecular basis of the disorder is known\",\"inheritances\":[\"Autosomal dominant\"]}]},{\"mimNumber\":300531,\"description\":\"Sprouty, Drosophila, homolog of, 3\"}],\"annotation2\":0.154}";

            var sb = StringBuilderCache.Acquire();
            annotatedGene.SerializeJson(sb);
            Assert.Equal(expectedLine, StringBuilderCache.GetStringAndRelease(sb));
        }
    }
}