using System.IO;
using System.Security.Cryptography;
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
        private readonly FileMetadata _metadata = new FileMetadata(new byte[] { 0, 1, 2, 3, 4, 5, 6 }, 1);
        private readonly string _filePath       = Resources.TopPath("clinvar.dict");

        private static Mock<IS3Client> GetS3ClientMock()
        {
            var s3ClientMock = new Mock<IS3Client>();
            s3ClientMock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>())).ReturnsAsync(new PutObjectResponse());
            return s3ClientMock;
        }

        [Fact]
        public void DecryptUpload_AsExpected()
        {
            Mock<IS3Client> s3ClientMock = GetS3ClientMock();

            using (var aes = new AesCryptoServiceProvider())
            {
                var s3Client = s3ClientMock.Object;
                s3Client.DecryptUpload("bucket", "bob.json.gz", _filePath, aes, _metadata);
            }

            s3ClientMock.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>()), Times.Once);
        }

        [Fact]
        public void DecryptUpload_FileNotFound()
        {
            Assert.Throws<FileNotFoundException>(delegate
            {
                Mock<IS3Client> s3ClientMock = GetS3ClientMock();

                using (var aes = new AesCryptoServiceProvider())
                {
                    var s3Client = s3ClientMock.Object;
                    s3Client.DecryptUpload("bucket", "bob.json.gz", "bob123", aes, _metadata);
                }
            });
        }
    }
}