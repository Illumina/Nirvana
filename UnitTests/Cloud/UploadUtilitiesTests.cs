using Amazon.S3.Model;
using Cloud;
using IO;
using Moq;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.Cloud
{
    public sealed class UploadUtilitiesTests
    {
        private readonly string _localFilePath = Resources.TopPath("Mother_chr22.genome.vcf.gz.tbi");

        [Fact]
        public void GetMd5Base64_AsExpected()
        {
            string md5 = UploadUtilities.GetMd5Base64(_localFilePath);

            Assert.Equal("5R0SGqGHoTONijCK+Qb3PQ==", md5);
        }

        [Fact]
        public void GetPutObjectRequest_AsExpected()
        {
            const string bucketName = "Test";

            var putRequest = UploadUtilities.GetPutObjectRequest(bucketName, "/path/to/file.json.gz", _localFilePath);

            Assert.Equal(bucketName, putRequest.BucketName);
            Assert.Equal("path/to/file.json.gz", putRequest.Key);
            Assert.Equal(_localFilePath, putRequest.FilePath);
            Assert.Equal("5R0SGqGHoTONijCK+Qb3PQ==", putRequest.MD5Digest);
        }

        [Fact]
        public void Upload_AsExpected()
        {
            var s3ClientMock = new Mock<IS3Client>();

            s3ClientMock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>())).ReturnsAsync(new PutObjectResponse());

            UploadUtilities.Upload(s3ClientMock.Object, "bucket", "/path/to/file.json.gz", _localFilePath);

            s3ClientMock.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>()), Times.Once);
        }
    }
}