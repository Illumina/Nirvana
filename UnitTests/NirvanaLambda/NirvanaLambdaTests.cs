using ErrorHandling;
using NL = NirvanaLambda.NirvanaLambda;
using Xunit;

namespace UnitTests.NirvanaLambda
{
    public sealed class NirvanaLambdaTests
    {
        [Theory]
        [InlineData("/tmp/ada.vcf", 0, "ada_00000")]
        [InlineData("/ada.vcf", 1, "ada_00001")]
        [InlineData("ada.vcf", 2, "ada_00002")]
        [InlineData("ada.vcf.gz", 3, "ada_00003")]
        [InlineData("ada.vcf.data.vcf.gz", 4, "ada_00004")]
        [InlineData("https://s3.amazonaws.com/illumina-early-access-zeus/Olympia.vcf.gz?AWSAccessKeyId=AKISKSD87A3C4&Expires=109838429&Signature=s98df7s8df12f2jo4lfjfs9d0fu0sd9f", 5, "Olympia_00005")]
        [InlineData("https://stratus-gds-stage.s3.us-west-2.amazonaws.com/d3a56bf8-5528-4b4d-b5bb-08d6c9c1c9dd/test-data/vcf/some-chroms/dq/DQ-Strelka-Germline-chr22-hg38.vcf.gz?X-Amz-Expires=604800&response-content-disposition=attachment%3Bfilename%3D%22DQ-Strelka-Germline-chr22-hg38.vcf.gz%22&x-userId=fb2136c7-01c2-32cc-8d53-b78db2c022de&X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAJ7P2VLXQJYGXATTA/20190516/us-west-2/s3/aws4_request&X-Amz-Date=20190516T160606Z&X-Amz-SignedHeaders=host&X-Amz-Signature=8b2f512998b820e8fb18433b5fd2de1c189c157accff92d5d5316a9fa3684d19", 6, "DQ-Strelka-Germline-chr22-hg38_00006")]
        public void GetIndexedPrefix_AsExpected(string inputVcfPath, int jobIndex, string expectedPrefix)
        {
            Assert.Equal(NL.GetIndexedPrefix(inputVcfPath, jobIndex), expectedPrefix);
        }

        [Theory]
        [InlineData(ErrorCategory.UserError, "Wrong input.", "User error: wrong input.")]
        [InlineData(ErrorCategory.NirvanaError, null, "Nirvana error: an unexpected annotation error occurred while annotating this VCF.")]
        [InlineData(ErrorCategory.TimeOutError, null, "Timeout error: annotation of the VCF was not finished on time due to network congestion. Please try again later.")]
        [InlineData(ErrorCategory.InvocationThrottledError, null, "Invocation throttled error: there are too many lambdas currently running in this account. Please try again later.")]
        public void GetFailedRunStatus_AsExpected(ErrorCategory errorCategory, string errorMessage, string expectedStatus)
        {
            Assert.Equal(expectedStatus, NL.GetFailedRunStatus(errorCategory, errorMessage));
        }
    }
}
