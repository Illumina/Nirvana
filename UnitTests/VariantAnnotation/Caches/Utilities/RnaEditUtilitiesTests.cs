using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Caches.Utilities;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.Caches.Utilities
{
    public sealed class RnaEditUtilitiesTests
    {
        [Theory]
        [InlineData(100, 100, "G", VariantType.SNV)]
        [InlineData(100, 101, "GT", VariantType.MNV)]
        [InlineData(101, 100, "GCTA", VariantType.insertion)]
        [InlineData(100, 100, "", VariantType.deletion)]
        [InlineData(100, 101, null, VariantType.deletion)]
        public void RnaEditTypes(int start, int end, string bases, VariantType expectedType)
        {
            var rnaEdit = new RnaEdit(start, end, bases);

            Assert.Equal(expectedType, RnaEditUtilities.GetRnaEditType(rnaEdit));
        }
    }
}