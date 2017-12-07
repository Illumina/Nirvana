using System.IO;
using System.Text;
using VariantAnnotation.IO;
using Xunit;
using VariantAnnotation.GeneAnnotation;

namespace UnitTests.VariantAnnotation.GeneAnnotation
{
    public sealed class GeneWriterAndReadTests
    {
        [Fact]
        public void Write_and_read_return_the_same_info()
        {
            var geneAnnotation = new GeneAnnotationSource("omim", new[] { "{\"mimNumber\":103950,\"description\":\"Alpha-2-macroglobulin\",\"phenotypes\":[{\"mimNumber\":614036,\"phenotype\":\"Alpha-2-macroglobulin deficiency\",\"mapping\":\"mapping of the wildtype gene\",\"inheritances\":[\"Autosomal dominant\"]}", "{\"mimNumber\":104300,\"phenotype\":\"Alzheimer disease, susceptibility to\",\"mapping\":\"molecular basis of the disorder is known\",\"inheritances\":[\"Autosomal dominant\"],\"comments\":\"contribute to susceptibility to multifactorial disorders or to susceptibility to infection\"}]}" }, true);
            var ms = new MemoryStream();
            using (var writer = new ExtendedBinaryWriter(ms, Encoding.Default, true))
            {
                geneAnnotation.Write(writer);
            }
               

            ms.Position = 0;
            var reader = new ExtendedBinaryReader(ms);
            var observedAnnotation = GeneAnnotationSource.Read(reader);

            Assert.Equal(geneAnnotation.DataSource ,observedAnnotation.DataSource);
            Assert.Equal(geneAnnotation.JsonStrings.Length,observedAnnotation.JsonStrings.Length);
            Assert.Equal(geneAnnotation.JsonStrings[0], observedAnnotation.JsonStrings[0]);
            Assert.Equal(geneAnnotation.JsonStrings[1], observedAnnotation.JsonStrings[1]);
            Assert.Equal(geneAnnotation.IsArray,observedAnnotation.IsArray);

        }

    }
}
