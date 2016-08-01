using System.IO;
using System.Linq;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling;
using Xunit;

namespace UnitTests.DataStructures
{
    public sealed class GeneTests
    {
        [Theory]
        [InlineData("ENST00000368232_chr1_Ensembl84.ndb", "GPATCH4", 156564279, 156571288)]
        [InlineData("ENST00000416839_chr1_Ensembl84.ndb", "AC096644.1", 220603286, 220608023)]
        [InlineData("ENST00000600779_chr1_Ensembl84.ndb", "AL589739.1", 2258581, 2259042)]
        public void GeneCoordinates(string cacheFileName, string expectedGeneSymbol, int expectedStart, int expectedEnd)
        {
            var dataStore              = new NirvanaDataStore();
            var transcriptIntervalTree = new IntervalTree<Transcript>();

            // populate the data store with our VEP annotations
            using (var reader = new NirvanaDatabaseReader(Path.Combine("Resources", "Caches", cacheFileName)))
            {
                reader.PopulateData(dataStore, transcriptIntervalTree);
            }

            // loop through the gene list
            Gene observedGene = dataStore.Genes.FirstOrDefault(gene => gene.Symbol == expectedGeneSymbol);

            Assert.NotNull(observedGene);

            // ReSharper disable PossibleNullReferenceException
            Assert.Equal(expectedGeneSymbol, observedGene.Symbol);
            Assert.Equal(expectedStart, observedGene.Start);
            Assert.Equal(expectedEnd, observedGene.End);
            // ReSharper restore PossibleNullReferenceException
        }
    }
}