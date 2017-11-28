using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.VariantAnnotation.Utilities
{
    public sealed class FormatUtilitiesTests
    {
        [Fact]
        public void CombineIdAndVersion()
        {
            const string expectedResult = "ENSG00000141510.7";
            var id = CompactId.Convert("ENSG00000141510");
            var observedResult = FormatUtilities.CombineIdAndVersion(id, 7);
            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void SplitVersion_ReturnNull_WithNullInput()
        {
            var result = FormatUtilities.SplitVersion(null);
            Assert.Null(result.Id);
            Assert.Equal(0, result.Version);
        }

        [Theory]
        [InlineData("ENSG00000141510.7", "ENSG00000141510", 7)]
        [InlineData("ENSG00000141510", "ENSG00000141510", 0)]
        public void SplitVersion(string combinedId, string expectedId, byte expectedVersion)
        {
            var result = FormatUtilities.SplitVersion(combinedId);
            Assert.Equal(expectedId, result.Id);
            Assert.Equal(expectedVersion, result.Version);
        }
    }
}
