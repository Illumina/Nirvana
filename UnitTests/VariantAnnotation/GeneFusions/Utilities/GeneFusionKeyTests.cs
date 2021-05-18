using VariantAnnotation.GeneFusions.Utilities;
using Xunit;

namespace UnitTests.VariantAnnotation.GeneFusions.Utilities
{
    public sealed class GeneFusionKeyTests
    {
        [Fact]
        public void Create_ExpectedResults()
        {
            const string geneA             = "ENSG00000006210";
            const string geneB             = "ENSG00000102962";
            const ulong  expectedFusionKey = 26671747011122;

            ulong actualFusionKey = GeneFusionKey.Create(geneA, geneB);
            Assert.Equal(expectedFusionKey, actualFusionKey);
        }

        [Theory]
        [InlineData("ENSG00000006210", null)]
        [InlineData(null,              "ENSG00000102962")]
        [InlineData(null,              null)]
        public void Create_OneGeneIsNull_ReturnZero(string geneA, string geneB)
        {
            const ulong expectedFusionKey = 0;
            ulong       actualFusionKey   = GeneFusionKey.Create(geneA, geneB);
            Assert.Equal(expectedFusionKey, actualFusionKey);
        }
    }
}