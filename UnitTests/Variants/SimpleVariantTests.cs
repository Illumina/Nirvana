using UnitTests.TestUtilities;
using Variants;
using Xunit;

namespace UnitTests.Variants
{
    public sealed class SimpleVariantTests
    {
        [Fact]
        public void SimpleVariant_Set()
        {
            const int expectedStart        = 100;
            const int expectedEnd          = 102;
            const string expectedRef       = "AT";
            const string expectedAlt       = "";
            const VariantType expectedType = VariantType.deletion;

            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, expectedStart, expectedEnd, expectedRef, expectedAlt, expectedType);

            Assert.Equal(ChromosomeUtilities.Chr1, variant.Chromosome);
            Assert.Equal(expectedStart,      variant.Start);
            Assert.Equal(expectedEnd,        variant.End);
            Assert.Equal(expectedRef,        variant.RefAllele);
            Assert.Equal(expectedAlt,        variant.AltAllele);
            Assert.Equal(expectedType,       variant.Type);
        }
    }
}
