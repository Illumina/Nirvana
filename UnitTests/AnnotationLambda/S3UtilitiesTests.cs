using AnnotationLambda;
using Xunit;

namespace UnitTests.AnnotationLambda
{
    public sealed class S3UtilitiesTests
    {
        [Theory]
        [InlineData("/Test/", "bob", "Test/bob")]
        [InlineData("Test/", "bob", "Test/bob")]
        [InlineData("/Test", "bob", "Test/bob")]
        [InlineData("Test", "bob", "Test/bob")]
        [InlineData("", "bob", "bob")]
        [InlineData(null, "bob", "bob")]
        [InlineData("/", "bob", "bob")]
        public void GetKey_Theory(string outputDir, string filename, string expectedResult)
        {
            var observedResult = S3Utilities.GetKey(outputDir, filename);
            Assert.Equal(expectedResult, observedResult);
        }
    }
}
