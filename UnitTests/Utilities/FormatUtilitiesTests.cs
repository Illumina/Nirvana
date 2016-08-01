using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.Utilities
{
    public class FormatUtilitiesTests
    {
        [Theory]
        [InlineData("NM_12345.1", 2, "NM_12345.1")]
        [InlineData("NM_12345", 2, "NM_12345.2")]
        [InlineData("NM_12345.BOB", 3, "NM_12345.BOB.3")]
        [InlineData("NM_12345.BOB.4", 5, "NM_12345.BOB.4")]
        public void GetVersion(string id, byte version, string expectedVersion)
        {
            var observedVersion = FormatUtilities.GetVersion(id, version);
            Assert.Equal(expectedVersion, observedVersion);
        }
    }
}
