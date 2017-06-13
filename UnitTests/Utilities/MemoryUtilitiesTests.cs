using CommandLine.Utilities;
using Xunit;

namespace UnitTests.Utilities
{
    public class MemoryUtilitiesTests
    {
        [Theory]
        [InlineData(10, "10 B")]
        [InlineData(10000, "9.8 KB")]
        [InlineData(10000000, "9.5 MB")]
        [InlineData(10000000000, "9.313 GB")]
        public void ToHumanReadable(long numBytes, string expectedResult)
        {
            Assert.Equal(expectedResult, MemoryUtilities.ToHumanReadable(numBytes));
        }
    }
}
