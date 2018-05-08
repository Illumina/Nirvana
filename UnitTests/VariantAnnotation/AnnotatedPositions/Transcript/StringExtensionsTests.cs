using VariantAnnotation.AnnotatedPositions.Transcript;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class StringExtensionsTests
    {
        [Theory]
        [InlineData(null,null,0)]
        [InlineData("abc",null,0)]
        [InlineData("abc", "abgg", 2)]
        [InlineData("abcfdg", "abgg", 2)]
        public void CommonPrefixLength(string a, string b, int expResult)
        {
            Assert.Equal(expResult,a.CommonPrefixLength(b));
        }


        [Theory]
        [InlineData(null, null, 0)]
        [InlineData("abc", null, 0)]
        [InlineData("abc", "abgg", 0)]
        [InlineData("abcfdg", "abgg", 1)]
        public void CommonSuffixLength(string a, string b, int expResult)
        {
            Assert.Equal(expResult, a.CommonSuffixLength(b));
        }
    }
}