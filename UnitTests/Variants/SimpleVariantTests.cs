using Genome;
using Variants;
using Xunit;

namespace UnitTests.Variants
{
    public sealed class SimpleVariantTests
    {
        [Fact]
        public void SimpleVariant_Set()
        {
            var expectedChromosome         = new Chromosome("chr1", "1", 0);
            const int expectedStart        = 100;
            const int expectedEnd          = 102;
            const string expectedRef       = "AT";
            const string expectedAlt       = "";
            const VariantType expectedType = VariantType.deletion;

            var variant = new SimpleVariant(expectedChromosome, expectedStart, expectedEnd, expectedRef, expectedAlt, expectedType);

            Assert.Equal(expectedChromosome, variant.Chromosome);
            Assert.Equal(expectedStart,      variant.Start);
            Assert.Equal(expectedEnd,        variant.End);
            Assert.Equal(expectedRef,        variant.RefAllele);
            Assert.Equal(expectedAlt,        variant.AltAllele);
            Assert.Equal(expectedType,       variant.Type);
        }
    }
}
