using Xunit;

namespace UnitTests.VariantAnnotation.PSA
{
    public sealed class ReaderWriterTests
    {
        
        [Fact]
        public void PsaReaderWriterTest()
        {
            using (var reader = PsaTestUtilities.GetSiftPsaReader())
            {
                Assert.Null(reader.GetScore(4, "gene4", "Trans-001", 12, 'K'));
                Assert.Null(reader.GetScore(0, "gene4", "Trans-001", 12, 'K'));
                Assert.Null(reader.GetScore(0, "GENE2", "Trans-001", 12, 'K'));
                
                Assert.NotNull(reader.GetScore(0, "GENE2", "ENST00000641515", 12, 'K'));
                Assert.NotNull(reader.GetScore(1, "GENE3", "ENST00000479739", 12, 'K'));
            }
        }
    }
}