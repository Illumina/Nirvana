using Amazon.S3;
using Amazon.S3.Model;
using Cloud;
using Moq;
using Xunit;

namespace UnitTests.Cloud
{
    public sealed class AmazonS3ClientWrapperTests
    {
        private readonly S3Path _testInputS3Path = new S3Path
        {
            bucketName = "test",
            path = "input.vcf.gz"
        };

        [Theory]
        [InlineData("ada", "/test/lambda/1.txt", "ada/test/lambda", "1.txt")]
        [InlineData("bob", "test/lambda/input/", "bob/test/lambda/input", "")]
        [InlineData("cathy", "test/lambda/input", "cathy/test/lambda", "input")]
        public void GetS3FileDirAndName_AsExecpted(string bucketName, string fullFilePath, string expectedBucketNameForGetRequest, string expectedS3FileName)
        {
            var s3Path = new S3Path
            {
                bucketName = bucketName,
                path = fullFilePath
            };
            var (s3Dir, s3FileName) = AmazonS3ClientWrapper.GetBucketAndFileNamesForS3Request(s3Path);
            Assert.Equal(s3Dir, expectedBucketNameForGetRequest);
            Assert.Equal(s3FileName, expectedS3FileName);
        }

        [Fact]
        public void GetFileSize_AsExpected()
        {
            long fileSize = 1000;
            var s3ClientMock = new Mock<IS3Client>();
            s3ClientMock.Setup(x => x.GetFileSize(_testInputS3Path)).Returns(fileSize);
        
            Assert.Equal(fileSize, s3ClientMock.Object.GetFileSize(_testInputS3Path));
        }

    }
}