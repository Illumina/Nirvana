using IO;
using Xunit;

namespace UnitTests.IO
{
    public class FilePathUtilitiesTests
    {
        [InlineData("C:\\Input files\\input.test.mp3", ".mp3", true)]
        [InlineData("C:\\Input files\\input", "C:\\Input files\\input", true)]
        [InlineData("\\\\ussd-prd-isi04\\Nirvana\\input.vcf", "vcf", false)]
        [InlineData("/d/Projects/Nirvana/input.vcf", ".vcf", true)]
        [InlineData("https://illumina.s3.amazonaws.com/input/Custom_SA/Custom-annotations_short-GRCh37.nsa?AWSAccessKeyId=UUNE5Q&Expires=asdf223&Signature=asdfasd", ".nsa", true)]
        [InlineData("https://stratus-gds-stage.s3.us-west-2.amazonaws.com/b9077f78-6b4e-4068-b4b2-08d6d80d1d7d/custom-filter-file/custom-annotation/2b8e155e-9046-4ef5-9ec0-374ccc98a93c/2b8e155e-9046-4ef5-9ec0-374ccc98a93c.nsa?X-Amz-Expires=604800&response-content-disposition=attachment%3Bfilename%3D%222b8e155e-9046-4ef5-9ec0-374ccc98a93c.nsa%22&x-userId=086723b2-1e53-32cd-a410-80cb885de66c&X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAJ7P2VLXQJYGXATTA/20190708/us-west-2/s3/aws4_request&X-Amz-Date=20190708T163940Z&X-Amz-SignedHeaders=host&X-Amz-Signature=d386f9d0aa7aab1a1a67c3ee625a208589924a51e384840ce9159a88b6c8363a", "nsa", false)]
        [Theory]
        public void GetFileSuffix_AsExpected(string filePath, string suffix, bool includeDot)
        {
            Assert.Equal(suffix, filePath.GetFileSuffix(includeDot));
        }
    }
}
