using IO;
using Xunit;

namespace UnitTests.IO
{
    public sealed class UrlUtilitiesTests
    {
        [Theory]
        [InlineData("http://www.illumina.com", "bob", "http://www.illumina.com/bob")]
        [InlineData("http://www.illumina.com/", "bob", "http://www.illumina.com/bob")]
        [InlineData("http://www.illumina.com", "/bob", "http://www.illumina.com/bob")]
        [InlineData("http://www.illumina.com/", "/bob", "http://www.illumina.com/bob")]
        public void Combine_Nominal(string prefix, string suffix, string expected)
        {
            string observed = prefix.UrlCombine(suffix);
            Assert.Equal(expected, observed);
        }

        [Fact]
        public void GetFileName_Nominal()
        {
            const string url = "https://illumina-usw2-olympia-dev.s3.amazonaws.com/Annotation/input/Mother.vcf.gz?AWSAccessKeyId=AKIAI774CQHRMUZUNE5Q&Signature=W7Rofh4%2BFXPrPE9ONrdk2iKrGqE%3D&Expires=1561072628";
            string observed = UrlUtilities.GetFileName(url);
            Assert.Equal("Mother.vcf.gz", observed);
        }
    }
}
