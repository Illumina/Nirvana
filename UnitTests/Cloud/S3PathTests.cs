using Cloud;
using ErrorHandling.Exceptions;
using Xunit;

namespace UnitTests.Cloud
{
    public sealed class S3PathTests
    {
        [Theory]
        [InlineData("/this/is/a/folder/", false)]
        [InlineData("/this/is/a/file", true)]
        public void ValidatePathFormat_AsExpected(string path, bool isDirectory)
        {
            Assert.Throws<UserErrorException>(() => S3Path.ValidatePathFormat(path, isDirectory));
        }

        [Fact]
        public void FormatPath_AsExpected()
        {
            Assert.Equal("to/the/file", S3Path.FormatPath("/to/the/file"));
            Assert.Equal("to/the/directory/", S3Path.FormatPath("/to/the/directory/"));
        }
    }
}

