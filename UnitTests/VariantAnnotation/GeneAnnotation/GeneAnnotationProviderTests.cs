using System;
using System.IO;
using SAUtils.MergeInterimTsvs;
using VariantAnnotation.GeneAnnotation;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.VariantAnnotation.GeneAnnotation
{
    public sealed class GeneAnnotationProviderTests
    {
        [Fact]
        public void Gene_in_gene_annotation_database_get_annotated()
        {
            var annotatedGene = new AnnotatedGene("A2M",
                new IGeneAnnotationSource[] { new GeneAnnotationSource("omim", new[] { "{\"mimNumber\":103950,\"description\":\"Alpha-2-macroglobulin\",\"phenotypes\":[{\"mimNumber\":614036,\"phenotype\":\"Alpha-2-macroglobulin deficiency\",\"mapping\":\"mapping of the wildtype gene\",\"inheritances\":[\"Autosomal dominant\"]}", "{\"mimNumber\":104300,\"phenotype\":\"Alzheimer disease, susceptibility to\",\"mapping\":\"molecular basis of the disorder is known\",\"inheritances\":[\"Autosomal dominant\"],\"comments\":\"contribute to susceptibility to multifactorial disorders or to susceptibility to infection\"}]}" }, true) });

            var ms = new MemoryStream();
            var header = new SupplementaryAnnotationHeader("", DateTime.Now.Ticks, 1, new IDataSourceVersion[] { }, GenomeAssembly.Unknown);
            using (var writer = new GeneDatabaseWriter(ms, header, true))
            {
                writer.Write(annotatedGene);
            }

            ms.Position = 0;
            var reader = new GeneDatabaseReader(ms);

            var geneAnnotationProvider = new GeneAnnotationProvider(reader);

            var observedAnnotation = geneAnnotationProvider.Annotate("A2M");
            var observedAnnotation2 = geneAnnotationProvider.Annotate("A2M2L");

            Assert.NotNull(observedAnnotation);
            Assert.Null(observedAnnotation2);
            Assert.Single(observedAnnotation.Annotations);
            Assert.Equal("omim", observedAnnotation.Annotations[0].DataSource);
        }
    }
}