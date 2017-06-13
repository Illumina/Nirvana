using System.Linq;
using UnitTests.Utilities;
using Xunit;

namespace UnitTests.VariantAnnotationTests.DataStructures
{
    public sealed class GeneTests
    {
        [Theory]
        [InlineData("ENST00000368232_chr1_Ensembl84", "GPATCH4", 156564100, 156571288)]
        [InlineData("ENST00000416839_chr1_Ensembl84", "AC096644.1", 220603286, 220608023)]
        [InlineData("ENST00000600779_chr1_Ensembl84", "AL589739.1", 2258581, 2259042)]
        public void GeneCoordinates(string cacheStub, string expectedGeneSymbol, int expectedStart, int expectedEnd)
        {
            var cache = DataUtilities.GetTranscriptCache(Resources.CacheGRCh37(cacheStub));

            var observedGene = cache.Genes.FirstOrDefault(gene => gene.Symbol == expectedGeneSymbol);
            Assert.NotNull(observedGene);

            Assert.Equal(expectedGeneSymbol, observedGene.Symbol);
            Assert.Equal(expectedStart, observedGene.Start);
            Assert.Equal(expectedEnd, observedGene.End);
        }
    }
}