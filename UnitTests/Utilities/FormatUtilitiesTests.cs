using VariantAnnotation.DataStructures;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.Utilities
{
    public class FormatUtilitiesTests
    {
        [Theory]
        [InlineData("NM_12345", 2, "NM_12345.2")]
        [InlineData("ENST00000344720", 3, "ENST00000344720.3")]
        public void CombineIdAndVersion(string id, byte version, string expectedVersion)
        {
            var compactId       = CompactId.Convert(id);
            var observedVersion = FormatUtilities.CombineIdAndVersion(compactId, version);
            Assert.Equal(expectedVersion, observedVersion);
        }

        [Theory]
        [InlineData("NM_12345.6", "NM_12345", 6)]
        [InlineData("ENST00000344720.3", "ENST00000344720", 3)]
        [InlineData("ENST00000344720", "ENST00000344720", 0)]
        public void SplitVersion(string originalId, string expectedId, byte expectedVersion)
        {
            var tuple = FormatUtilities.SplitVersion(originalId);
            Assert.Equal(expectedId,      tuple.Item1);
            Assert.Equal(expectedVersion, tuple.Item2);
        }
    }
}
