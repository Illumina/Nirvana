using System.IO;
using System.Text;
using IO;
using Xunit;
using VariantAnnotation.Interface.GeneAnnotation;

namespace UnitTests.VariantAnnotation.GeneAnnotation
{
    public sealed class GeneWriterAndReadTests
    {
        [Fact]
        public void Write_and_read_return_the_same_info()
        {
            var expectedGeneAnnotation = new global::VariantAnnotation.GeneAnnotation.GeneAnnotation("omim", new[] { "{\"mimNumber\":103950,\"description\":\"Alpha-2-macroglobulin\",\"phenotypes\":[{\"mimNumber\":614036,\"phenotype\":\"Alpha-2-macroglobulin deficiency\",\"mapping\":\"mapping of the wildtype gene\",\"inheritances\":[\"Autosomal dominant\"]}", "{\"mimNumber\":104300,\"phenotype\":\"Alzheimer disease, susceptibility to\",\"mapping\":\"molecular basis of the disorder is known\",\"inheritances\":[\"Autosomal dominant\"],\"comments\":\"contribute to susceptibility to multifactorial disorders or to susceptibility to infection\"}]}" }, true);

            IGeneAnnotation observedGeneAnnotation;

            using (var ms = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(ms, Encoding.Default, true))
                {
                    expectedGeneAnnotation.Write(writer);
                }

                ms.Position = 0;

                using (var reader = new ExtendedBinaryReader(ms))
                {
                    observedGeneAnnotation = global::VariantAnnotation.GeneAnnotation.GeneAnnotation.Read(reader);
                }
            }

            Assert.Equal(expectedGeneAnnotation.DataSource, observedGeneAnnotation.DataSource);
            Assert.Equal(expectedGeneAnnotation.JsonStrings.Length, observedGeneAnnotation.JsonStrings.Length);
            Assert.Equal(expectedGeneAnnotation.JsonStrings[0], observedGeneAnnotation.JsonStrings[0]);
            Assert.Equal(expectedGeneAnnotation.JsonStrings[1], observedGeneAnnotation.JsonStrings[1]);
            Assert.Equal(expectedGeneAnnotation.IsArray, observedGeneAnnotation.IsArray);
        }
    }
}
